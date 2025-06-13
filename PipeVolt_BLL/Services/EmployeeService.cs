using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.DTOS.PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PipeVolt_DAL.Common.DataType;

namespace PipeVolt_BLL.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericRepository<Employee> _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IGenericRepository<UserAccount> _userAccountRepo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public EmployeeService(IGenericRepository<Employee> repo,
                               IEmployeeRepository employeeRepo,
                               IGenericRepository<UserAccount> userAccountRepo,
                               ILoggerService logger,
                               IMapper mapper)
        {
            _repo = repo; _logger = logger; _mapper = mapper; _employeeRepo = employeeRepo; _userAccountRepo = userAccountRepo ?? throw new ArgumentNullException(nameof(userAccountRepo));
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var ents = await _repo.GetAll();
            return _mapper.Map<List<EmployeeDto>>(ents);
        }
        public async Task<EmployeeDto> GetByIdAsync(int id)
        {
            var q = await _repo.QueryBy(e => e.EmployeeId == id);
            var ent = q.FirstOrDefault()
                      ?? throw new KeyNotFoundException("Employee not found");
            return _mapper.Map<EmployeeDto>(ent);
        }
        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
        {
            var ent = _mapper.Map<Employee>(dto);
            ent.EmployeeCode = await _employeeRepo.GenderCodeEmployee(ent.EmployeeId);
            var created = await _repo.Create(ent);
            return _mapper.Map<EmployeeDto>(created);
        }
        public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            if (id != dto.EmployeeId) throw new ArgumentException("ID mismatch");
            var q = await _repo.QueryBy(e => e.EmployeeId == id);
            var ent = q.FirstOrDefault()
                      ?? throw new KeyNotFoundException("Employee not found");
            _mapper.Map(dto, ent);
            await _repo.Update(ent);
            return _mapper.Map<EmployeeDto>(ent);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var q = await _repo.QueryBy(e => e.EmployeeId == id);
            var ent = q.FirstOrDefault()
                      ?? throw new KeyNotFoundException("Employee not found");
            await _repo.Delete(ent);
            return true;
        }
        public async Task<UserAccountDto> GenerateUserAccountForEmployee(int employeeId)
        {
            var employees = await _repo.QueryBy(e => e.EmployeeId == employeeId);
            var employee = employees.FirstOrDefault();

            if (employee==null)
            {
                _logger.LogWarning($"Employee with ID {employeeId} not found.");
                throw new KeyNotFoundException("Employee not found");

            }
            var account = new UserAccount
            {
                Username = employee.EmployeeCode,
                Password = BCrypt.Net.BCrypt.HashPassword(employee.EmployeeCode),
                UserType =(int)UserType.Employee,
                EmployeeId = employee.EmployeeId,
                Status =(int)UserStatus.Active,
            };
            await _userAccountRepo.Create(account);

            return _mapper.Map<UserAccountDto>(account);
        }
    }
}
