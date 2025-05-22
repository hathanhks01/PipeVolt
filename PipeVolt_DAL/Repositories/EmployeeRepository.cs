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
    public class EmployeeRepository
    : GenericRepository<Employee>, IGenericRepository<Employee>, IEmployeeRepository
    {
        PipeVoltDbContext _context;
        public EmployeeRepository(IUnitOfWork uow) : base(uow)
        {
            _context = new PipeVoltDbContext();
        }

        public async Task<string> GenderCodeEmployee(int id)
        {
            int employeeCount = await _context.Employees.CountAsync();
            string code = "NV" + (employeeCount + 1);

            return code;
        }

    }

}
