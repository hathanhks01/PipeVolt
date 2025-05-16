using Microsoft.EntityFrameworkCore;

namespace PipeVolt_Api.Common.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        Task<int> Commit();

        DbContext Context { get; }
    }
}
