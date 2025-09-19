using cybersoft_final_project.Models.Request;
using SWD392_backend.Entities;
using SWD392_backend.Entities.Enums;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.OrderService;

public interface IOrderService
{   
    Task<bool> CheckoutAsync(OrderCheckoutDTO orderDTO, int userId);

    Task<object> GetOrdersByRoleAsync(string role, int id, int page, int pageSize);

    Task<bool> UpdateOrderItemStatusAsync(UpdateOrderItemStatus dto);


    Task<int> GetTotalOrdersAsync();

    Task<PagedResult<OrderResponse>> GetOrdersToShipper(int userId, int pageNumber, int pageSize);
    Task UpdateOrderStatus(string orderId, OrderStatus status);
    Task<ReportOrderResponse> CountOrdersByMonthAsync(int month, int year, int pageNumber, int pageSize);
    Task<ReportOrderResponse> CountOrdersByDayAsync(int day, int month, int year, int pageNumber, int pageSize);

    Task<(int totalOrders, int totalUsers)> GetSummaryThisMonthAsync();

    Task AssignShipperToOrderAsync(Guid orderId);
}