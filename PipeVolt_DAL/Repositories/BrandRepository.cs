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
    public class BrandRepository : GenericRepository<Brand>, IGenericRepository<Brand>
    {
        public BrandRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
