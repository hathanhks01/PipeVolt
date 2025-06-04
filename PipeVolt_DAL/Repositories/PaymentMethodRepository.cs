using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class PaymentMethodRepository:GenericRepository<PaymentMethod>, IGenericRepository<PaymentMethod>
    {
        public PaymentMethodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
