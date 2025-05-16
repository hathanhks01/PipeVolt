using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL.Models;
using System;
using System.Threading.Tasks;

namespace PipeVolt_Api.Common.Repository
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly PipeVoltDbContext _context;

        public UnitOfWork(PipeVoltDbContext context)
        {
            _context = context;
        }

        public DbContext Context => _context;

        // Bổ sung <T> ở đây
        public DbSet<T> Set<T>() where T : class
        {
            return _context.Set<T>();
        }

        // Trả về int để khớp với Task<int> Commit()
        public async Task<int> Commit()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

}