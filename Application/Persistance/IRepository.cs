using System.Linq.Expressions;

namespace Application.Persistance
{
    public interface IRepository<TEntity, TId> where TEntity : class
    {
        TEntity? Get(TId id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

    }
}
