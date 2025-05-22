using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS.PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IGenericRepository<UserAccount> _repo;
        private readonly IUserAccountRepository _userAccountRepo; // Dùng cho FindByUsernameAsync
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public UserAccountService(
            IGenericRepository<UserAccount> repo,
            IUserAccountRepository userAccountRepo,
            ILoggerService loggerService,
            IMapper mapper)
        {
            _repo = repo;
            _userAccountRepo = userAccountRepo;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        public async Task<List<UserAccountDto>> GetAllUserAccountsAsync()
        {
            try
            {
                var userAccounts = await _repo.GetAll();
                _loggerService.LogInformation("Fetched all user accounts successfully");
                return _mapper.Map<List<UserAccountDto>>(userAccounts);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error fetching all user accounts", ex);
                throw;
            }
        }

        public async Task<UserAccountDto> GetUserAccountByIdAsync(int id)
        {
            try
            {
                var userAccounts = await _repo.QueryBy(u => u.UserId == id);
                var entity = await Task.Run(() => userAccounts.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("User account not found.");
                return _mapper.Map<UserAccountDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error fetching user account with ID {id}", ex);
                throw;
            }
        }

        public async Task<UserAccountDto> GetUserAccountByUsernameAsync(string username)
        {
            try
            {
                var entity = await _userAccountRepo.FindByUsernameAsync(username);
                if (entity == null)
                    throw new KeyNotFoundException("User account not found.");
                return _mapper.Map<UserAccountDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error fetching user account with username {username}", ex);
                throw;
            }
        }

        public async Task<UserAccountDto> AddUserAccountAsync(CreateUserAccountDto userAccount)
        {
            try
            {
                var entity = _mapper.Map<UserAccount>(userAccount);
                await _repo.Create(entity);
                _loggerService.LogInformation($"User account with ID {entity.UserId} added successfully");
                return _mapper.Map<UserAccountDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error adding user account", ex);
                throw;
            }
        }

        public async Task<UserAccountDto> UpdateUserAccountAsync(int id, UpdateUserAccountDto userAccount)
        {
            try
            {
                var existingUserAccounts = await _repo.QueryBy(u => u.UserId == id);
                var entity = await Task.Run(() => existingUserAccounts.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("User account not found.");
                _mapper.Map(userAccount, entity);
                await _repo.Update(entity);
                _loggerService.LogInformation($"User account with ID {id} updated successfully");
                return _mapper.Map<UserAccountDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error updating user account with ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteUserAccountAsync(int id)
        {
            try
            {
                var userAccounts = await _repo.QueryBy(u => u.UserId == id);
                var entity = await Task.Run(() => userAccounts.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("User account not found.");
                await _repo.Delete(entity);
                _loggerService.LogInformation($"User account with ID {id} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error deleting user account with ID {id}", ex);
                return false;
            }
        }
    }
}
