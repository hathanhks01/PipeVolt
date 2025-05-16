using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class InventoryRepository : GenericRepository<Inventory>, IGenericRepository<Inventory>
    {
        public InventoryRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
