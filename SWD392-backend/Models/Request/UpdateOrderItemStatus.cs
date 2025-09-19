using SWD392_backend.Entities.Enums;

namespace SWD392_backend.Models.Request;

public class UpdateOrderItemStatus
{
    public string OrderDetailId { get; set; }
    public int ProductId { get; set; }  // dùng để xác định từng item
    public OrderStatus NewStatus { get; set; }

}