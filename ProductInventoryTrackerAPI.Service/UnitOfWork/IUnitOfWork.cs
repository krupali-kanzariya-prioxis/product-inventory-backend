using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductInventoryTrackerAPI.Service.RepositoryFactory;

namespace ProductInventoryTrackerAPI.Service.UnitOfWork

{
    public interface IUnitOfWork : IDisposable
    {

        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        int Commit(bool autoHistory = false);
        Task<int> CommitAsync(bool autoHistory = false);
        Task<int> CommitAsyncWithTransaction();
        void ClearContext();
        IDbContextTransaction dbContextTransaction { get; set; }
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
    }
}
