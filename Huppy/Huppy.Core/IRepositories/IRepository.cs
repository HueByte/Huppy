namespace Huppy.Core.IRepositories
{
    public interface IRepository<Tkey, TEntity>
    {
        Task<TEntity?> GetAsync(Tkey id);
        Task<IQueryable<TEntity>> GetAllAsync();
        Task<bool> AddAsync(TEntity? entity);
        Task<bool> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<bool> RemoveAsync(Tkey id);
        Task<bool> RemoveAsync(TEntity? entity);
        Task UpdateAsync(TEntity? entity);
        Task UpdateRange(IEnumerable<TEntity> entities);
        Task SaveChangesAsync();
    }
}