namespace SWD392_backend.Models.Response;

public class OrderResponse
{
    public Guid Id { get; set; }
    public double Total { get; set; }
    public int UserId { get; set; }              
    public string UserFullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Address { get; set; } = null!;
    public double ShippingPrice { get; set; }
    public int SupplierId { get; set; }          
    public string SupplierName { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DeliveriedAt { get; set; }

    public List<OrderDetailResponse> orders_details { get; set; } = new();
}