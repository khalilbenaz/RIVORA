namespace RVR.SaaS.Starter.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string? ShippingAddress { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
