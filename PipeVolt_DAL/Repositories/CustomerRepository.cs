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
            int customerCount = await _context.Customers.CountAsync();

            string code = "KH" + (customerCount + 1); 

            return code;  
        }
    }
}
