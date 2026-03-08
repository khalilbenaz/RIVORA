using Serilog;
using MassTransit;
using KBA.Microservices.Shared.Application.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<OrderConfirmedEventConsumer>();
    x.AddConsumer<OrderCancelledEventConsumer>();
    x.AddConsumer<UserRegisteredEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ReceiveEndpoint("order-created-queue", e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        cfg.ReceiveEndpoint("order-confirmed-queue", e => e.ConfigureConsumer<OrderConfirmedEventConsumer>(context));
        cfg.ReceiveEndpoint("order-cancelled-queue", e => e.ConfigureConsumer<OrderCancelledEventConsumer>(context));
        cfg.ReceiveEndpoint("user-registered-queue", e => e.ConfigureConsumer<UserRegisteredEventConsumer>(context));
    });
});

var app = builder.Build();

Log.Information("Starting NotificationService");
app.Run();

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation($"Sending order confirmation email for order {context.Message.OrderNumber}");
        // TODO: Implement actual email sending
        return Task.CompletedTask;
    }
}

public class OrderConfirmedEventConsumer : IConsumer<OrderConfirmedEvent>
{
    private readonly ILogger<OrderConfirmedEventConsumer> _logger;

    public OrderConfirmedEventConsumer(ILogger<OrderConfirmedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        _logger.LogInformation($"Sending order confirmed notification for {context.Message.OrderNumber}");
        return Task.CompletedTask;
    }
}

public class OrderCancelledEventConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventConsumer> _logger;

    public OrderCancelledEventConsumer(ILogger<OrderCancelledEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        _logger.LogInformation($"Sending order cancellation notification for {context.Message.OrderNumber}: {context.Message.Reason}");
        return Task.CompletedTask;
    }
}

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(ILogger<UserRegisteredEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        _logger.LogInformation($"Sending welcome email to {context.Message.Email}");
        return Task.CompletedTask;
    }
}
