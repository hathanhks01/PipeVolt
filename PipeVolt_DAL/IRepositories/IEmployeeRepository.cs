namespace PipeVolt_DAL.IRepositories
{
    public interface IEmployeeRepository
    {
        Task<string> GenderCodeEmployee(int id);
    }
}