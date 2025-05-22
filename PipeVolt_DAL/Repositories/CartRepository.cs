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
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
        {
            var query = await QueryBy(c => c.CustomerId == customerId);
            return await query.Include(c => c.CartItems)
                             .ThenInclude(ci => ci.Product)
                             .FirstOrDefaultAsync();
        }
    }
}
