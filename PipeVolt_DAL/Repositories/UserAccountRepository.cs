using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly PipeVoltDbContext _context;

        public UserAccountRepository(PipeVoltDbContext context)
        {
            _context = context;
        }

        public async Task<UserAccount?> FindByUsernameAsync(string username)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(UserAccount user)
        {
            await _context.UserAccounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(UserAccount user)
        {
            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
