namespace cybersoft_final_project.Models.Request;

public class OrderDetailDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public double DiscountPercent { get; set; }
    public string Note { get; set; } = "";
}