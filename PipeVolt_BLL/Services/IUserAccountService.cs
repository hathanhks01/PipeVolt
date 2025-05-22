using PipeVolt_DAL.DTOS.PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface IUserAccountService
    {
        Task<UserAccountDto> AddUserAccountAsync(CreateUserAccountDto userAccount);
        Task<bool> DeleteUserAccountAsync(int id);
        Task<List<UserAccountDto>> GetAllUserAccountsAsync();
        Task<UserAccountDto> GetUserAccountByIdAsync(int id);
        Task<UserAccountDto> GetUserAccountByUsernameAsync(string username);
        Task<UserAccountDto> UpdateUserAccountAsync(int id, UpdateUserAccountDto userAccount);
    }
}