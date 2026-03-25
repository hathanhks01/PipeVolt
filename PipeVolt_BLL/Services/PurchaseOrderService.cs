using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using Microsoft.AspNetCore.Http;
using PipeVolt_DAL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PipeVolt_BLL.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IGenericRepository<PurchaseOrder> _repo;
        private readonly IGenericRepository<PurchaseOrderDetail> _detailRepo;
        private readonly IGenericRepository<UserAccount> _userRepo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PurchaseOrderService(
            IGenericRepository<PurchaseOrder> repo,
            IGenericRepository<PurchaseOrderDetail> detailRepo,
            ILoggerService logger,
            IMapper mapper,
            IGenericRepository<UserAccount> userRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<List<PurchaseOrderDto>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all purchase orders");
                var entities = await _repo.GetAll();
                var result = _mapper.Map<List<PurchaseOrderDto>>(entities);
                _logger.LogInformation($"Fetched {result.Count} purchase orders");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching purchase orders", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }
                var dto = _mapper.Map<PurchaseOrderDto>(entity);
                _logger.LogInformation($"Fetched purchase order ID {id}");
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching purchase order ID {id}", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> AddPurchaseOrderAsync(CreatePurchaseOrderDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new purchase order");
                var entity = _mapper.Map<PurchaseOrder>(dto);

                entity.EmployeeId = await ResolveEmployeeIdFromLoggedInUserAsync();

                // Tự sinh mã đơn theo ngày (ví dụ: PC032526). Nếu trùng prefix trong ngày
                // thì thêm hậu tố để đảm bảo không bị trùng.
                entity.PurchaseOrderCode = await GeneratePurchaseOrderCodeAsync();

                // Default status nếu FE chưa gửi
                entity.Status ??= (int)PipeVolt_DAL.Common.DataType.PurchaseOrderStatus.Draft;

                var created = await _repo.Create(entity);
                
                // Thêm Details nếu có
                if (dto.Details != null && dto.Details.Count > 0)
                {
                    foreach (var detailDto in dto.Details)
                    {
                        var detailEntity = new PurchaseOrderDetail
                        {
                            PurchaseOrderId = created.PurchaseOrderId,
                            ProductId = detailDto.ProductId,
                            Quantity = detailDto.Quantity,
                            UnitCost = detailDto.UnitCost
                        };
                        await _detailRepo.Create(detailEntity);
                    }
                    _logger.LogInformation($"Added {dto.Details.Count} purchase order details");
                }

                var result = _mapper.Map<PurchaseOrderDto>(created);
                _logger.LogInformation($"Added purchase order ID {result.PurchaseOrderId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding purchase order", ex);
                throw;
            }
        }

        private async Task<int> ResolveEmployeeIdFromLoggedInUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User == null) throw new UnauthorizedAccessException("Missing http context user");

            var claims = httpContext.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            //_logger.LogError($"Available claims: {string.Join(", ", claims)}");

            var sub =
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(sub)) throw new UnauthorizedAccessException($"Missing user id claim. Available claims: {string.Join(", ", claims)}");

            if (!int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid user id claim");

            // Tìm user để đọc user_type và employee_id
            var user = (await _userRepo.FindBy(u => u.UserId == userId)).FirstOrDefault();
            if (user == null) throw new UnauthorizedAccessException("User not found");

            if (user.UserType == (int)DataType.UserType.Admin)
            {
                // Admin không gắn với employee, theo yêu cầu set = 1
                return 1;
            }

            if (user.UserType == (int)DataType.UserType.Employee)
            {
                if (user.EmployeeId == null)
                    throw new InvalidOperationException("Employee user does not have EmployeeId");

                return user.EmployeeId.Value;
            }

            throw new UnauthorizedAccessException("User is not allowed to create purchase orders");
        }

        private async Task<string> GeneratePurchaseOrderCodeAsync()
        {
            var now = DateTime.Now;
            var prefix = "PC" + now.ToString("MMddyy");

            var existing = await _repo.FindBy(
                x => x.PurchaseOrderCode != null && x.PurchaseOrderCode.StartsWith(prefix)
            );

            // Lấy max suffix từ các mã cùng ngày, +1 để tạo mã mới
            var maxSuffix = -1;
            foreach (var code in existing.Select(x => x.PurchaseOrderCode))
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var num))
                {
                    maxSuffix = Math.Max(maxSuffix, num);
                }
            }

            var nextNumber = maxSuffix + 1;
            
            // Format động: 000, 001, ..., 099, 100, ..., 999, 1000, ...
            var suffixLength = nextNumber < 1000 ? 3 : nextNumber.ToString().Length;
            return $"{prefix}{nextNumber.ToString().PadLeft(suffixLength, '0')}";
        }

        public async Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(int id, UpdatePurchaseOrderDto dto)
        {
            if (dto == null || dto.PurchaseOrderId != id)
            {
                _logger.LogWarning("Invalid update request for purchase order");
                throw new ArgumentException("PurchaseOrder ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }

                _mapper.Map(dto, entity);

                // Luôn đồng bộ EmployeeId theo user đang đăng nhập
                entity.EmployeeId = await ResolveEmployeeIdFromLoggedInUserAsync();

                await _repo.Update(entity);
                var result = _mapper.Map<PurchaseOrderDto>(entity);
                _logger.LogInformation($"Updated purchase order ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating purchase order ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeletePurchaseOrderAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted purchase order ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting purchase order ID {id}", ex);
                throw;
            }
        }
    }
}
