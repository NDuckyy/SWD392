using SWD392_backend.Models.Request;

namespace cybersoft_final_project.Models.Request;

public class OrderCheckoutDTO
{
    public int SupplierId { get; set; }
    public string Address { get; set; } = "";
    public AssignAreaRequest assignAreaRequest { get; set; }
    public double Distance { get; set; }
    public double Total { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; } = new();
}