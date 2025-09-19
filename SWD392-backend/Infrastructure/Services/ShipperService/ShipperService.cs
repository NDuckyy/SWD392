using AutoMapper;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.ShipperRepository;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ShipperService
{
    public class ShipperService : IShipperService
    {
        private readonly IShipperRepository _shipperRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipperService(IShipperRepository shipperRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _shipperRepository = shipperRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> AssignAreaAsync(int userId, AssignAreaRequest request)
        {
            var shipper = await _shipperRepository.GetShipperByUserIdAsync(userId);
            if (shipper == null)
                return false;

            var areaCode = $"{request.ProvinceCode}_{request.DistrictCode}_{request.WardCode}";

            shipper.AreaCode = areaCode;

            await _shipperRepository.AssignAreaAsync(shipper);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<List<shipper>> GetAllShippers(string areaCode)
        {
            return await _shipperRepository.GetAllShipper(areaCode);
        }

        public async Task<OrderResponse> GetOrderByIdAsync(int id, Guid orderId)
        {
            var shipper = await _shipperRepository.GetShipperByUserIdAsync(id);
            if (shipper == null)
                return null;

            var order = await _shipperRepository.GetOrderByIdAsync(orderId);

            var orderDto = _mapper.Map<OrderResponse>(order);

            return orderDto;
        }

        public async Task<ShipperProfileResponse> GetShipperByUserIdAsync(int userId)
        {
            var shipper = await _shipperRepository.GetShipperByUserIdAsync(userId);

            if (shipper == null)
                return null;

            var shipperDto = _mapper.Map<ShipperProfileResponse>(shipper);

            return shipperDto;
        }
    }
}
