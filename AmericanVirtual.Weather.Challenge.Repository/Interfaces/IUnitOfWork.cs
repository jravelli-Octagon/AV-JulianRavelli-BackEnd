using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace AmericanVirtual.Weather.Challenge.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IDbContextTransaction Transaction { get; }
        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void CommitTransaction();
        void RollbackTransaction();
        bool HasTransaction { get; }
        Task<List<T>> ExecuteFunctionList<T>(string spName, params object[] parameters) where T : class, new();
        Task<string> ExecuteFunctionSingleRecord(string spName, params object[] parameters);
        Task<bool> ExecuteFunctionNonQuery(string spName, List<SqlParameter> parameters);
        DataSet ExecuteFunctionNonQueryDS(string spName, List<SqlParameter> parameters);
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
    }
}