using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.UserRepository;
using SWD392_backend.Models;
using SWD392_backend.Models.Requests;
using SWD392_backend.Models.Response;
using web_api_base.Helper;


namespace SWD392_backend.Infrastructure.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<UserProfileResponse>> GetAllUserAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _userRepository.GetAllUserAsync(pageNumber, pageSize);

            // Model mapper
            var userDtos = _mapper.Map<List<UserProfileResponse>>(pagedResult.Items);


            return new PagedResult<UserProfileResponse>
            {
                Items = userDtos,
                TotalItems = pagedResult.TotalItems,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<UserProfileResponse> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                return null;

            var userDto = _mapper.Map<UserProfileResponse>(user);

            return userDto;
        }

        public async Task<int> GetTotalUserAsync()
        {
            return await _unitOfWork.UserRepository.CountAsync();
        }

        public async Task AddUserAsync(UserRequest request)
        {
            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                throw new Exception("Email đã tồn tại trong hệ thống.");
            }

            var user = new user
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.Fullname,
                ImageUrl = "https://i.pravatar.cc/300",
                Password = PasswordHelper.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                Phone = request.Phone,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Không thể thêm người dùng vào cơ sở dữ liệu. Chi tiết: " + dbEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi không xác định khi thêm người dùng. " + ex.Message);
            }
        }
        
        
        public async Task<int> GetTotalUsersByMonth(int month, int year)
        {
            return await _userRepository.GetTotalUserByMonth(month, year);
        }

        public async Task<int> GetUserCountByExactDay(DateTime day)
        {
            var start = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);

            return await _userRepository.CountUsersBetween(start, end);
        }
        
        
        public async Task<user> UpdateUserStatusAsync(int userId)
        {
            return await _userRepository.UpdateUserStatusAsync(userId);
        }


    }
}