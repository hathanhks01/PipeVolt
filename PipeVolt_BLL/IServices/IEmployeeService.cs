using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto> GetByIdAsync(int id);
        Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
    }
}