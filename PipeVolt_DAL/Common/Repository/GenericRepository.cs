using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace PipeVolt_Api.Common.Repository
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        internal DbSet<T> _dbset;
        private readonly IUnitOfWork _unitOfWork;


        protected GenericRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbset = _unitOfWork.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await Task.Run(() => _dbset.AsEnumerable());
        }

        public async Task<IQueryable<T>> QueryAll()
        {
            return await Task.Run(() => _dbset.AsQueryable());
        }

        public async Task<int> Count(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() => _dbset.Where(predicate).Count());
        }

        public async Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() => _dbset.Where(predicate).AsNoTracking().AsEnumerable());
        }

        public async Task<IQueryable<T>> QueryBy(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() => _dbset.Where(predicate));
        }
        public async Task<IQueryable<T>> QueryBy(Expression<Func<T, bool>> predicate, bool tracking = true)
        {
            if (tracking)
            {
                return await Task.Run(() => _dbset.Where(predicate));
            }
            else
            {
                return await Task.Run(() => _dbset.Where(predicate).AsNoTracking());
            }

        }

        public async Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate, Expression<Func<T, dynamic>> orderBy, int pagesize, int pageindex)
        {
            return await Task.Run(() => _dbset.Where(predicate).OrderBy(orderBy).Skip(pagesize * (pageindex - 1)).Take(pagesize).AsEnumerable());
        }
        public async Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate, string[] childrens)
        {
            IQueryable<T> query = _dbset;
            if (childrens != null && childrens.Length > 0)
            {
                foreach (var children in childrens)
                {
                    query = query.Include(children);
                }
            }
            return await Task.Run(() => query.Where(predicate).AsEnumerable());
        }
        public virtual async Task<T> Create(T entity)
        {
            var rt = _dbset.Add(entity);
            await Save();
            await Detach(entity);
            return rt.Entity;
        }
        public virtual async Task<int> Create(List<T> lstEntity)
        {
            foreach (T item in lstEntity)
            {
                _dbset.Add(item);
            }

            return await Save();
        }
        public virtual async Task<int> Update(T entity)
        {
            //_dbset.Attach(entity);
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
            var rsSave = await Save();
            await Detach(entity);
            return rsSave;

        }

        public virtual async Task<int> UpdateNoSave(T entity)
        {
            _dbset.Attach(entity);
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(1);
        }

        public async Task<int> Delete(T entity)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Deleted;
            //_dbset.Remove(entity);
            return await Save();
        }

        public async Task<int> DeleteRange(Expression<Func<T, bool>> predicate)
        {
            var todelete = _dbset.Where(predicate);
            _dbset.RemoveRange(todelete);
            return await Save();
        }

        public async Task<int> Save()
        {
            return await _unitOfWork.Commit();
        }
        public async Task Detach(T entity)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Detached;
        }

        /// <summary>
        /// Query db trả ra list objects <para/>
        /// VD: <code> SqlQuery<SSD_User>("select * from ssd_user where uername = @uname and s_location = @division", new SqlParameter("uname", "thien"), SqlParameter("division", "fis"))</code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">sql text, VD: select * from ssd_user where uername = @uname </param>
        /// <param name="rawParameters"> List sql paramneter, VD new new SqlParameter("uname", param1) </param>
        /// <returns></returns>
        public async Task<List<TEntity>> SqlQuery<TEntity>(string query, params object[] rawParameters) where TEntity : class
        {
            // Sử dụng phương thức mới của EF Core để thực hiện truy vấn SQL
            return await _unitOfWork.Context.Set<TEntity>().FromSqlRaw(query, rawParameters).ToListAsync();
        }

        public async Task<int> ExcuteStoreProcedure(string storeName, params object[] parameters)
        {
            if ((parameters.Length % 2) != 0) throw new Exception("Invalid parameters!");
            var pCmd = storeName + " ";
            var cmdArr = new List<SqlParameter>();
            for (var i = 0; i < parameters.Length; i = i + 2)
            {
                pCmd += parameters[i] + ", ";
                cmdArr.Add(new SqlParameter((string)parameters[i], parameters[i + 1]));
            }
            pCmd = pCmd.Trim(' ').Trim(',');

            // Sử dụng ExecuteSqlRawAsync thay vì ExecuteSqlCommandAsync
            var dt = await _unitOfWork.Context.Database.ExecuteSqlRawAsync(pCmd, cmdArr.Cast<object>().ToArray());
            return dt;
        }

        public async Task<DataTable> ExcuteStoreProcedureToTable(string storeName, params object[] parameters)
        {
            DataTable dt = new DataTable();

            // Truy cập connection thông qua GetDbConnection()
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = (SqlConnection)_unitOfWork.Context.Database.GetDbConnection();
                var pCmd = storeName + " ";
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

                for (var i = 0; i < parameters.Length; i = i + 2)
                {
                    pCmd += parameters[i] + ", ";
                    cmd.Parameters.Add(new SqlParameter((string)parameters[i], parameters[i + 1]));
                }
                pCmd = pCmd.Trim(' ').Trim(',');
                cmd.CommandText = pCmd;
                var da = new SqlDataAdapter { SelectCommand = cmd };
                da.Fill(dt);
            }
            return dt;
        }

        public async Task<DataSet> ExcuteStoreProcedureToDataSet(string storeName, params object[] parameters)
        {
            DataSet dt = new DataSet();

            // Truy cập connection thông qua GetDbConnection()
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = (SqlConnection)_unitOfWork.Context.Database.GetDbConnection();
                var pCmd = storeName + " ";
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

                for (var i = 0; i < parameters.Length; i = i + 2)
                {
                    pCmd += parameters[i] + ", ";
                    cmd.Parameters.Add(new SqlParameter((string)parameters[i], parameters[i + 1]));
                }
                pCmd = pCmd.Trim(' ').Trim(',');
                cmd.CommandText = pCmd;
                var da = new SqlDataAdapter { SelectCommand = cmd };
                da.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// Excute thủ tục db trả ra list objects. gọi như code kiểu cũ <para/>
        /// VD: <code> ExcuteStoreProcedureToList<SSD_User>("SSD_User_GetByUserName", "@UserName", "thiendd14")</code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storeName">tên thủ tục, VD: SSD_User_GetByUserName </param>
        /// <param name="parameters"> List paramneter. gồm 2 cặp tên và giá trị </param>
        /// <returns></returns>
        public async Task<List<TEntity>> ExcuteStoreProcedureToList<TEntity>(string storeName, params object[] parameters) where TEntity : class
        {
            if ((parameters.Length % 2) != 0) throw new Exception("Invalid parameters!");
            var pCmd = storeName + " ";
            var cmdArr = new List<SqlParameter>();
            for (var i = 0; i < parameters.Length; i = i + 2)
            {
                pCmd += parameters[i] + ", ";
                cmdArr.Add(new SqlParameter((string)parameters[i], parameters[i + 1]));
            }
            pCmd = pCmd.Trim(' ').Trim(',');

            // Sử dụng FromSqlRaw thay vì SqlQuery
            var query = _unitOfWork.Context.Set<TEntity>().FromSqlRaw(pCmd, cmdArr.Cast<object>().ToArray());
            return await query.ToListAsync();
        }

        public async Task<TEntity> ExcuteStoreProcedureToValue<TEntity>(string storeName, params object[] parameters) where TEntity : class
        {
            if ((parameters.Length % 2) != 0) throw new Exception("Invalid parameters!");
            var pCmd = storeName + " ";
            var cmdArr = new List<SqlParameter>();
            for (var i = 0; i < parameters.Length; i = i + 2)
            {
                pCmd += parameters[i] + ", ";
                cmdArr.Add(new SqlParameter((string)parameters[i], parameters[i + 1]));
            }
            pCmd = pCmd.Trim(' ').Trim(',');

            // Sử dụng FromSqlRaw thay vì SqlQuery
            var query = _unitOfWork.Context.Set<TEntity>().FromSqlRaw(pCmd, cmdArr.Cast<object>().ToArray());
            return await query.FirstOrDefaultAsync();
        }
    }
}