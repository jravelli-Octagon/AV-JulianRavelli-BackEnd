using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using AmericanVirtual.Weather.Challenge.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Snickler.EFCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AmericanVirtual.Weather.Challenge.Repository
{
    public abstract class BaseRepository<T> : IReadRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DbContext context)
        {
            _dbContext = context ?? throw new ArgumentException(nameof(context));
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<List<T>> GetList(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true, int? maximumNumberOfResults = null)
        {
            IQueryable<T> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            query = orderBy != null ? orderBy(query) : query;

            query = maximumNumberOfResults == null ? query : query.Take<T>(maximumNumberOfResults.Value);

            return await query.ToListAsync();
        }

        public async Task<List<TResult>> GetList<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true) where TResult : class
        {
            IQueryable<T> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            var result = orderBy != null
                ? orderBy(query).Select(selector)
                : query.Select(selector);

            return await result.ToListAsync();
        }

        public virtual IQueryable<T> Query(string sql, params object[] parameters)
        {
            return _dbSet.FromSqlRaw(sql, parameters);
        }

        public T Search(params object[] keyValues) => _dbSet.Find(keyValues);

        public async Task<T> Single(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).FirstOrDefaultAsync();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> ExecuteFunction<T>(string spName, List<SqlParameter> parameters) where T : class, new()
        {
            _dbContext.Database.SetCommandTimeout(120);

            IList<T> result = new List<T>();
            var SqlCommand = _dbContext.LoadStoredProc(spName);

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    SqlCommand.Parameters.Add(item);
                }
            }

            await SqlCommand.ExecuteStoredProcAsync((handler) =>
            {
                result = handler.ReadToList<T>();
            }).ConfigureAwait(false);
            return result.ToList();
        }
    }
}