using AutoMapper;
using cybersoft_final_project.Models.Request;
using Elastic.Clients.Elasticsearch.MachineLearning;
using Elastic.Clients.Elasticsearch.Requests;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Entities;
using SWD392_backend.Entities.Enums;
using SWD392_backend.Infrastructure.Repositories.OrderRepository;
using SWD392_backend.Infrastructure.Services.ShipperService;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.OrderService;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    private readonly IShipperService _shipperService;
    private readonly IOrderRepository _orderRepository;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IShipperService shipperService,
        IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _shipperService = shipperService;
        _orderRepository = orderRepository;
    }


    public async Task<bool> CheckoutAsync(OrderCheckoutDTO orderDTO, int userId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var areaCode =
                $"{orderDTO.assignAreaRequest.ProvinceCode}_{orderDTO.assignAreaRequest.DistrictCode}_{orderDTO.assignAreaRequest.WardCode}";

            var newOrder = new order()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SupplierId = orderDTO.SupplierId,
                Address = orderDTO.Address,
                AreaCode = areaCode,
                ShippingPrice = CalculateShippingPrice(orderDTO.Distance),
                Total = orderDTO.Total,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.OrderRepository.AddAsync(newOrder);
            await _unitOfWork.SaveAsync();

            foreach (var detail in orderDTO.OrderDetails)
            {
                // Lấy product theo id
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(detail.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {detail.ProductId} not found.");

                // Kiểm tra tồn kho đủ
                if (product.StockInQuantity < detail.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product ID {detail.ProductId}.");

                // Trừ tồn kho
                product.StockInQuantity -= detail.Quantity;

                // Cập nhật product trong DB
                _unitOfWork.ProductRepository.Update(product);

                // Thêm chi tiết đơn hàng
                var orderDetail = new orders_detail
                {
                    OrderId = newOrder.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    DiscountPercent = detail.DiscountPercent,
                    Note = detail.Note,
                    Status = OrderStatus.Pending
                };


                await _unitOfWork.OrdersDetailRepository.AddAsync(orderDetail);

                var order = await _orderRepository.GetOrderByIdAsync(orderDetail.OrderId);

                order.PaidAt = DateTime.UtcNow;

                _unitOfWork.OrderRepository.Update(order);
            }

            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private double CalculateShippingPrice(double distance)
    {
        if (distance <= 0)
            return 0;
        else if (distance <= 5)
            return distance * 10000; // ví dụ: 10.000đ/km cho <= 5km
        else if (distance <= 20)
            return distance * 8000; // 8.000đ/km
        else
            return distance * 5000; // 5.000đ/km
    }



    public async Task<object> GetOrdersByRoleAsync(string role, int id, int page, int pageSize)
    {
        IQueryable<order> query;

        if (role == "CUSTOMER")
        {
            query = _unitOfWork.OrderRepository
                .GetAll()
                .Include(order => order.orders_details)
                    .ThenInclude(od => od.product)
                        .ThenInclude(od => od.product_images)
                .Include(o => o.user)
                .Include(o => o.supplier)
                .Where(o => o.UserId == id);
            Console.WriteLine(query.ToString());
        }
        else if (role == "SUPPLIER")
        {
            query = _unitOfWork.OrderRepository
                .GetAll()
                .Include(order => order.orders_details)
                .Where(o => o.SupplierId == id);
            Console.WriteLine(query.ToString());
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid role.");
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderDTOs = _mapper.Map<List<OrderResponse>>(orders);

        return new
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = orderDTOs
        };
    }

    public async Task<bool> UpdateOrderItemStatusAsync(UpdateOrderItemStatus dto)
    {
        if (!int.TryParse(dto.OrderDetailId, out int orderDetailId))
        {
            throw new ArgumentException("Invalid OrderDetailId");
        }

        var orderDetail = await _unitOfWork.OrdersDetailRepository
            .GetAll()
            .AsQueryable()
            .FirstOrDefaultAsync(od => od.Id == orderDetailId && od.ProductId == dto.ProductId);


        if (orderDetail == null)
            throw new KeyNotFoundException("Order item not found.");

        if (orderDetail.Status == dto.NewStatus)
            return false;

        orderDetail.Status = dto.NewStatus;
        _unitOfWork.OrdersDetailRepository.Update(orderDetail);

        await _unitOfWork.SaveAsync();
        return true;
    }


    public async Task<int> GetTotalOrdersAsync()
    {
        return await _unitOfWork.OrderRepository.GetTotalOrdersAsync();
    }

    public async Task<PagedResult<OrderResponse>> GetOrdersToShipper(int userId, int pageNumber, int pageSize)
    {
        var shipper = await _shipperService.GetShipperByUserIdAsync(userId);

        if (shipper == null)
            return new PagedResult<OrderResponse>
            {
                Items = null,
                TotalItems = 0,
                Page = 0,
                PageSize = 0
            };

        var pagedResult = await _orderRepository.GetOrdersToShipperAsync(shipper.AreaCode, shipper.Id, pageNumber, pageSize);

        var ordersDtos = _mapper.Map<List<OrderResponse>>(pagedResult.Items);

        return new PagedResult<OrderResponse>
        {
            Items = ordersDtos,
            TotalItems = pagedResult.TotalItems,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task UpdateOrderStatus(string orderId, OrderStatus status)
    {
        var list = await _unitOfWork.OrderRepository.GetOrdersDetail(orderId);
        foreach (var orderDetail in list)
        {
            orderDetail.Status = status;
            _unitOfWork.OrdersDetailRepository.Update(orderDetail);
        }

        if (status == OrderStatus.Delivered)
        {
            var order = await _orderRepository.GetOrderByIdAsync(Guid.Parse(orderId));
            order.DeliveriedAt = DateTime.UtcNow;
            _unitOfWork.OrderRepository.Update(order);
        }

        if (status == OrderStatus.Preparing)
        {
            await AssignShipperToOrderAsync(Guid.Parse(orderId));
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<ReportOrderResponse> CountOrdersByMonthAsync(int month, int year, int pageNumber, int pageSize)
    {
        // Validate
        if (year < 2000 || year > DateTime.Now.Year + 1)
        {
            throw new ArgumentException("Year out of range");
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12");
        }

        var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var count = await _orderRepository.CountOrdersByMonthAsync(startDate, endDate);
        var orders = await _orderRepository.GetOrdersByMonthAsync(startDate, endDate, pageNumber, pageSize);

        var ordersDtos = _mapper.Map<List<OrderResponse>>(orders.Items);

        return new ReportOrderResponse
        {
            year = year,
            month = month,
            TotalOrders = orders.TotalItems,
            orders = new PagedResult<OrderResponse>
            {
                Items = ordersDtos,
                TotalItems = orders.TotalItems,
                Page = pageNumber,
                PageSize = pageSize
            }
        };
    }

    public async Task<ReportOrderResponse> CountOrdersByDayAsync(int day, int month, int year, int pageNumber, int pageSize)
    {
        if (year < 2000 || year > DateTime.Now.Year + 1)
        {
            throw new ArgumentException("Year is out of range.");
        }
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }
        if (day < 1 || day > 31)
        {
            throw new ArgumentException("Day must be between 1 and 31.");
        }

        if (!DateTime.TryParse($"{year}-{month}-{day}", out var dateOnly))
        {
            throw new ArgumentException("Invalid date.");
        }

        var startDate = DateTime.SpecifyKind(new DateTime(year, month, day, 0, 0, 0), DateTimeKind.Utc);
        var endDate = startDate.AddDays(1);

        var count = await _orderRepository.CountOrdersByDayAsync(startDate, endDate);
        var orders = await _orderRepository.GetOrdersByDayAsync(startDate, endDate, pageNumber, pageSize);

        var ordersDtos = _mapper.Map<List<OrderResponse>>(orders.Items);

        return new ReportOrderResponse
        {
            year = year,
            month = month,
            day = day,
            TotalOrders = orders.TotalItems,
            orders = new PagedResult<OrderResponse>
            {
                Items = ordersDtos,
                TotalItems = orders.TotalItems,
                Page = pageNumber,
                PageSize = pageSize
            }
        };

    }

    public async Task<(int totalOrders, int totalUsers)> GetSummaryThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = now;

        var totalOrders = await _orderRepository.CountOrdersInRangeAsync(startDate, endDate);
        var totalUsers = await _orderRepository.CountNewUsersInRangeAsync(startDate, endDate);

        return (totalOrders, totalUsers);
    }

    public async Task AssignShipperToOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null)
            throw new Exception("Order not found");

        if (order.ShipperId != null)
            throw new Exception("Order already assigned");

        var shippersInArea = await _shipperService.GetAllShippers(order.AreaCode);

        if (shippersInArea == null || shippersInArea.Count == 0)
            throw new Exception("No shipper available in this area.");

        var ordersInArea = await _orderRepository.GetAll()
                                .Where(o => o.AreaCode == order.AreaCode && o.ShipperId != null)
                                .Include(o => o.orders_details)
                                .ToListAsync();

        var activeOrdersByShipper = ordersInArea
                                        .GroupBy(o => o.ShipperId)
                                        .Select(g => new
                                        {
                                            ShipperId = g.Key,
                                            Count = g.Sum(o => o.orders_details.Count(od => od.Status == OrderStatus.Preparing || od.Status == OrderStatus.Delivery || od.Status == OrderStatus.Delivered  || od.Status == OrderStatus.Refunding || od.Status == OrderStatus.Returned || od.Status == OrderStatus.Refunded || od.Status == OrderStatus.Cancelled))
                                        })
                                        .ToList();

        var selectedShipper = shippersInArea
                                .Select(shipper => new
                                {
                                    Shipper = shipper,
                                    OrderCount = activeOrdersByShipper.FirstOrDefault(o => o.ShipperId == shipper.Id)?.Count ?? 0
                                })
                                .OrderBy(x => x.OrderCount)
                                .FirstOrDefault();

        if (selectedShipper == null)
            throw new Exception("No available shipper found.");

        order.ShipperId = selectedShipper.Shipper.Id;

        _unitOfWork.OrderRepository.Update(order);
        await _unitOfWork.SaveAsync();
    }
}