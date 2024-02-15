using AmericanVirtual.Weather.Challenge.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Snickler.EFCore;
using System.Data;
using System.Data.Common;

namespace AmericanVirtual.Weather.Challenge.Repository
{
    public class UnitOfWork<TContext> : IRepositoryFactory, IUnitOfWork<TContext>, IUnitOfWork
        where TContext : DbContext, IDisposable
    {
        private Dictionary<Type, object> _repositories;

        public UnitOfWork(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TContext Context { get; }

        public IDbContextTransaction Transaction { get; private set; }

        public bool HasTransaction => Transaction != null;

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new Dictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type)) _repositories[type] = new Repository<TEntity>(Context);
            return (IRepository<TEntity>)_repositories[type];
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (HasTransaction) return Transaction;

            return Transaction = Context.Database.BeginTransaction(isolationLevel);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (HasTransaction) return Transaction;

            return Transaction = await Context.Database.BeginTransactionAsync(isolationLevel);
        }

        public void CommitTransaction()
        {
            if (!HasTransaction) throw new NullReferenceException("Please call `BeginTransaction()` method first.");

            try
            {
                Transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            if (!HasTransaction) throw new NullReferenceException("Please call `BeginTransaction()` method first.");

            try
            {
                Transaction.Rollback();
            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
        }

        public async Task<List<T>> ExecuteFunctionList<T>(string spName, params object[] parameters) where T : class, new()
        {
            IList<T> result = new List<T>();

            await Context.LoadStoredProc(spName)
               .ExecuteStoredProcAsync((handler) =>
               {
                   result = handler.ReadToList<T>();
               }).ConfigureAwait(false);

            return result.ToList();
        }

        public async Task<string> ExecuteFunctionSingleRecord(string spName, params object[] parameters)
        {
            DbParameter outputParam = null;
            
            await Context.LoadStoredProc(spName)
                .WithSqlParam("@processorId", (dbParam) =>
                {
                    dbParam.Direction = System.Data.ParameterDirection.Output;
                    dbParam.Size = 150;
                    dbParam.DbType = System.Data.DbType.String;
                    outputParam = dbParam;
                }).ExecuteStoredNonQueryAsync().ConfigureAwait(false);

            return (string)outputParam?.Value;
        }

        public async Task<bool> ExecuteFunctionNonQuery(string spName, List<SqlParameter> parameters)
        {
            try
            {
                Context.Database.SetCommandTimeout(600);
                var SqlCommand = Context.LoadStoredProc(spName);
                SqlCommand.CommandTimeout = 600;
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        SqlCommand.Parameters.Add(item);
                    }
                }
                await SqlCommand.ExecuteStoredNonQueryAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public DataSet ExecuteFunctionNonQueryDS(string spName, List<SqlParameter> parameters)
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(Context.Database.GetDbConnection().ConnectionString))
            {
                SqlCommand sqlComm = new SqlCommand(spName, conn);

                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        sqlComm.Parameters.Add(item);
                    }
                }

                sqlComm.CommandType = CommandType.StoredProcedure;
                sqlComm.CommandTimeout = 600;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;

                da.Fill(ds);
            }

            return ds;
        }
    }
}