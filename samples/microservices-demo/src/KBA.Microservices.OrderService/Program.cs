using Serilog;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Grpc.Net.Client;
using KBA.Microservices.Shared.Domain.Entities;
using KBA.Microservices.Shared.Application.Events;
using KBA.Microservices.Shared.Infrastructure.Messaging;
using KBA.Microservices.Shared.Grpc;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=postgres_orders;Database=orders_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connectionString));

// gRPC clients
builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:ProductService"] ?? "http://product-service:80");
});

builder.Services.AddGrpcClient<IdentityService.IdentityServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:IdentityService"] ?? "http://identity-service:80");
});

// Saga for Order Checkout
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.AddSagaStateMachine<OrderStateMachine, OrderSaga>()
        .EntityFrameworkRepository(opt =>
        {
            opt.ExistingDbContext<OrderDbContext>();
            opt.UsePostgres();
            opt.RowVersionOrder = OrderSagaVersion.LastModified;
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IEventBus, MassTransitEventBus>();
builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

Log.Information("Starting OrderService");
app.Run();

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSaga> OrderSagas => Set<OrderSaga>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.OwnsMany(e => e.Items);
        });
        
        modelBuilder.Entity<OrderSaga>(entity =>
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(e => e.RowVersion).IsRowVersion();
        });
    }
}

public class OrderSaga : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public bool StockReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderSaga>
{
    public State Pending { get; set; } = null!;
    public State StockReserved { get; set; } = null!;
    public State PaymentProcessed { get; set; } = null!;
    public State Completed { get; set; } = null!;
    public State Failed { get; set; } = null!;

    public Event<OrderCreatedEvent> OrderCreated { get; set; } = null!;
    public Event<StockReservedEvent> StockReservedEvent { get; set; } = null!;
    public Event<StockReservationFailedEvent> StockFailed { get; set; } = null!;
    public Event<PaymentProcessedEvent> PaymentProcessedEvent { get; set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailed { get; set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.OrderId));
        Event(() => StockReservedEvent, e => e.CorrelateById(m => m.Message.OrderId));
        Event(() => StockFailed, e => e.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessedEvent, e => e.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentFailed, e => e.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderCreated)
                .TransitionTo(Pending)
                .ThenAsync(async context =>
                {
                    // Reserve stock via gRPC
                    // TODO: Implement actual gRPC call
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                })
                .PublishAsync(context => context.Init<OrderStockReservationRequestedEvent>(new
                {
                    OrderId = context.Message.OrderId,
                    Items = context.Message.Items
                }))
        );

        During(Pending,
            When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Then(context => context.Saga.StockReserved = true)
                .PublishAsync(context => context.Init<OrderPaymentRequestedEvent>(new
                {
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    Amount = context.Saga.TotalAmount
                })),
            When(StockFailed)
                .TransitionTo(Failed)
                .PublishAsync(context => context.Init<OrderCancelledEvent>(new
                {
                    OrderId = context.Saga.OrderId,
                    Reason = "Stock reservation failed"
                }))
        );

        During(StockReserved,
            When(PaymentProcessedEvent)
                .TransitionTo(Completed)
                .Then(context => context.Saga.PaymentProcessed = true)
                .PublishAsync(context => context.Init<OrderConfirmedEvent>(new
                {
                    OrderId = context.Saga.OrderId
                })),
            When(PaymentFailed)
                .TransitionTo(Failed)
                .PublishAsync(context => context.Init<StockReleaseRequestedEvent>(new
                {
                    OrderId = context.Saga.OrderId
                }))
                .PublishAsync(context => context.Init<OrderCancelledEvent>(new
                {
                    OrderId = context.Saga.OrderId,
                    Reason = "Payment failed"
                }))
        );
    }
}

// Event classes for Saga
public class StockReservedEvent { public Guid OrderId { get; set; } }
public class StockReservationFailedEvent { public Guid OrderId { get; set; } public string Reason { get; set; } = string.Empty; }
public class PaymentProcessedEvent { public Guid OrderId { get; set; } }
public class PaymentFailedEvent { public Guid OrderId { get; set; } public string Reason { get; set; } = string.Empty; }
public class OrderStockReservationRequestedEvent { public Guid OrderId { get; set; } public List<OrderItemDto> Items { get; set; } = new(); }
public class OrderPaymentRequestedEvent { public Guid OrderId { get; set; } public Guid CustomerId { get; set; } public decimal Amount { get; set; } }
public class StockReleaseRequestedEvent { public Guid OrderId { get; set; } }
