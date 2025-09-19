using Microsoft.EntityFrameworkCore.Storage;
using SWD392_backend.Context;
using SWD392_backend.Infrastructure.Repositories.CategoryRepository;
using SWD392_backend.Infrastructure.Repositories.OrderDetailRepository;
using SWD392_backend.Infrastructure.Repositories.OrderRepository;
using SWD392_backend.Infrastructure.Repositories.ProductImageRepository;
using SWD392_backend.Infrastructure.Repositories.ProductRepository;
using SWD392_backend.Infrastructure.Repositories.UserRepository;
using System.Threading.Tasks;
using SWD392_backend.Infrastructure.Repositories.SupplierRepository;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyDbContext _context;
    private IDbContextTransaction _transaction;

    public IUserRepository UserRepository { get; }
    public IOrderRepository OrderRepository { get; }
    public IOrdersDetailRepository OrdersDetailRepository { get; }
    public IProductRepository ProductRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IProductImageRepository ProductImageRepository { get; }
    public ISupplierRepository SupplierRepository { get; }

    public UnitOfWork(
        MyDbContext context,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IOrdersDetailRepository ordersDetailRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductImageRepository productImageRepository,
        ISupplierRepository supplierRepository)
    {
        _context = context;
        UserRepository = userRepository;
        OrderRepository = orderRepository;
        OrdersDetailRepository = ordersDetailRepository;
        ProductRepository = productRepository;
        CategoryRepository = categoryRepository;
        ProductImageRepository = productImageRepository;
        SupplierRepository = supplierRepository;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        await _context.SaveChangesAsync();
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}