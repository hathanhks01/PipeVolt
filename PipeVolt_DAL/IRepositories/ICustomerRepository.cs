using PipeVolt_DAL.Models;

namespace PipeVolt_DAL.IRepositories
{
    public interface ICustomerRepository
    {
        Task<string> RenderCodeAsync();
    }
}