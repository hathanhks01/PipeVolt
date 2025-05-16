using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PipeVolt_Api.Common.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<IQueryable<T>> QueryAll();
        Task<int> Count(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> QueryBy(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> QueryBy(Expression<Func<T, bool>> predicate, bool tracking);
        Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate, Expression<Func<T, dynamic>> orderBy, int pagesize, int pageindex);
        Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate, string[] childrens);
        Task<T> Create(T entity);
        Task<int> Create(List<T> entities);
        Task<int> Update(T entity);
        Task<int> UpdateNoSave(T entity);
        Task<int> Delete(T entity);
        Task<int> DeleteRange(Expression<Func<T, bool>> predicate);
        Task<int> Save();
        Task Detach(T entity);

        // Các phương thức cập nhật cho phù hợp với .NET 8.0
        Task<List<TEntity>> SqlQuery<TEntity>(string query, params object[] rawParameters) where TEntity : class;
        Task<int> ExcuteStoreProcedure(string storeName, params object[] parameters);
        Task<DataTable> ExcuteStoreProcedureToTable(string storeName, params object[] parameters);
        Task<DataSet> ExcuteStoreProcedureToDataSet(string storeName, params object[] parameters);
        Task<List<TEntity>> ExcuteStoreProcedureToList<TEntity>(string storeName, params object[] parameters) where TEntity : class;
        Task<TEntity> ExcuteStoreProcedureToValue<TEntity>(string storeName, params object[] parameters) where TEntity : class;
    }
}