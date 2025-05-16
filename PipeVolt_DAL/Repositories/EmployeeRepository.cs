using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Repositories
{
    public class EmployeeRepository
    : GenericRepository<Employee>, IGenericRepository<Employee>
    {
        public EmployeeRepository(IUnitOfWork uow) : base(uow) { }
    }

}
