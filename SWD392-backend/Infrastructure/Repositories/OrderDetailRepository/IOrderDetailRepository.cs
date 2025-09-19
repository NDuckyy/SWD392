namespace SWD392_backend.Infrastructure.Repositories.OrderDetailRepository;

// IOrdersDetailRepository.cs
using SWD392_backend.Entities;
using System.Threading.Tasks;

public interface IOrdersDetailRepository
{
    Task AddAsync(orders_detail entity);
    // Thêm các method khác nếu cần
    IQueryable<orders_detail> GetAll();
    
    void Update(orders_detail entity);


}
