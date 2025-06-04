using Microsoft.EntityFrameworkCore;
using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<CartItem> GetCartItemByCartAndProductAsync(int cartId, int productId)
        {
            var query = await QueryBy(ci => ci.CartId == cartId && ci.ProductId == productId);
            return await query.FirstOrDefaultAsync();
        }
        public async Task DeleteRange(Func<CartItem, bool> predicate)
        {
            var itemsToDelete = await QueryBy(ci => predicate(ci));
            if (!await itemsToDelete.AnyAsync())
            {
                throw new InvalidOperationException("Không có mục nào để xóa.");
            }
            await base.DeleteRange(ci => predicate(ci));
        }
    }
}
