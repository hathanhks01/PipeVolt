namespace PipeVolt_BLL.IServices
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
        void Set<T>(string key, T value, TimeSpan? duration = null);
    }
}