using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Persistance
{
    public interface IRepository<TEntity, TId> where TEntity : class
    {
        Task<TEntity?> Get(TId id);
        Task<IEnumerable<TEntity>> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        Task Add(TEntity entity);
        Task AddRange(IEnumerable<TEntity> entities);

        Task Update(TEntity entity);

        Task Remove(TEntity entity);
        Task RemoveRange(IEnumerable<TEntity> entities);

    }
}
