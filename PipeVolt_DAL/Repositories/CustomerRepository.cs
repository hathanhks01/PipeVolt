using Microsoft.EntityFrameworkCore;
using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, IGenericRepository<Customer>,ICustomerRepository
    {
        private readonly PipeVoltDbContext _context;

        public CustomerRepository(PipeVoltDbContext context, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _context = context;
        }
        public async Task<string> RenderCodeAsync()
        {
            var maxCode = await _context.Customers
                .Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith("KH"))
                .Select(c => c.CustomerCode)
                .OrderByDescending(c => c)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (maxCode != null && int.TryParse(maxCode.Substring(2), out int current))
            {
                nextNumber = current + 1;
            }

            return "KH" + nextNumber;
        }
    }
}
