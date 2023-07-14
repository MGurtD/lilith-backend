using System.Linq.Expressions;

namespace Application.Persistance.Repositories
{
    public interface IContractReader<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    }
}
