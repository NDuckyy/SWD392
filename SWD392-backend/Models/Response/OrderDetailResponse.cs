namespace SWD392_backend.Models.Response;

public class OrderDetailResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }      
    public string ProductImage { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public double DiscountPercent { get; set; }
    public string Note { get; set; } = null!;
    public string Status { get; set; } = null!;

}