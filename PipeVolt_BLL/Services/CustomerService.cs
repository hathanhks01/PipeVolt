using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IGenericRepository<Customer> _repo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public CustomerService(
            IGenericRepository<Customer> repo,
             ICustomerRepository customerRepo,
            ILoggerService loggerService,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                _loggerService.LogInformation("Fetching all customers");
                var customers = await _repo.GetAll();
                var result = _mapper.Map<List<CustomerDto>>(customers);
                _loggerService.LogInformation($"Fetched {result.Count} customers");
                return result;
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error fetching all customers", ex);
                throw;
            }
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            try
            {
                _loggerService.LogInformation($"Fetching customer with ID {id}");
                var customers = await _repo.QueryBy(p => p.CustomerId == id);
                var entity = await Task.Run(() => customers.FirstOrDefault());
                if (entity == null)
                {
                    _loggerService.LogWarning($"Customer with ID {id} not found");
                    throw new KeyNotFoundException("Customer not found.");
                }
                var result = _mapper.Map<CustomerDto>(entity);
                _loggerService.LogInformation($"Fetched customer with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error fetching customer with ID {id}", ex);
                throw;
            }
        }

        public async Task<CustomerDto> AddCustomerAsync(CreateCustomerDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.CustomerName))
            {
                _loggerService.LogWarning("Invalid customer data: Customer name is required");
                throw new ArgumentException("Customer name is required.");
            }

            try
            {
                _loggerService.LogInformation($"Adding new customer: {dto.CustomerName}");
                var entity = _mapper.Map<Customer>(dto);
                entity.RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow);
                entity.CustomerCode = await _customerRepo.RenderCodeAsync();
                var createdEntity = await _repo.Create(entity);
                var result = _mapper.Map<CustomerDto>(createdEntity);
                _loggerService.LogInformation($"Added customer with ID {result.CustomerId}");
                return result;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error adding customer: {dto.CustomerName}", ex);
                throw;
            }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
        {
            if (dto == null || dto.CustomerId != id || string.IsNullOrEmpty(dto.CustomerName))
            {
                _loggerService.LogWarning($"Invalid customer data: ID mismatch or customer name is required for ID {id}");
                throw new ArgumentException("Customer ID mismatch or customer name is required.");
            }

            try
            {
                _loggerService.LogInformation($"Updating customer with ID {id}");
                var customers = await _repo.QueryBy(p => p.CustomerId == id);
                var entity = await Task.Run(() => customers.FirstOrDefault());
                if (entity == null)
                {
                    _loggerService.LogWarning($"Customer with ID {id} not found");
                    throw new KeyNotFoundException("Customer not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<CustomerDto>(entity);
                _loggerService.LogInformation($"Updated customer with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error updating customer with ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            try
            {
                _loggerService.LogInformation($"Deleting customer with ID {customerId}");
                var customers = await _repo.QueryBy(p => p.CustomerId == customerId);
                var entity = await Task.Run(() => customers.FirstOrDefault());
                if (entity == null)
                {
                    _loggerService.LogWarning($"Customer with ID {customerId} not found");
                    throw new KeyNotFoundException("Customer not found.");
                }

                // Kiểm tra nếu có dữ liệu liên quan (nếu cần, ví dụ: đơn hàng)
                // var orderRepo = _repo.Set<Order>();
                // var hasOrders = await orderRepo.AnyAsync(o => o.CustomerId == customerId);
                // if (hasOrders)
                //     throw new InvalidOperationException("Cannot delete customer with associated orders.");

                await _repo.Delete(entity);
                _loggerService.LogInformation($"Deleted customer with ID {customerId}");
                return true;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error deleting customer with ID {customerId}", ex);
                throw;
            }
        }
    }
}