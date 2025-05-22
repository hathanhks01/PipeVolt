using Microsoft.EntityFrameworkCore;
using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;

namespace PipeVolt_DAL.Repositories
{
    public class UserAccountRepository : GenericRepository<UserAccount>, IGenericRepository<UserAccount>, IUserAccountRepository
    {
        private readonly PipeVoltDbContext _context;
        public UserAccountRepository(IUnitOfWork unitOfWork, PipeVoltDbContext context)
            : base(unitOfWork)
        {
            _context = context;
        }
        public async Task<UserAccount?> FindByUsernameAsync(string username)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
