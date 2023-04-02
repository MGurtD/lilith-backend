using Application.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories
{
    public class Repository<Entity, Id> : IRepository<Entity, Id> where Entity : class
    {
        protected ApplicationDbContext context;
        internal DbSet<Entity> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<Entity>();
        }

        public Entity? Get(Id id)
        {
            return dbSet.Find(id);
        }

        public IEnumerable<Entity> GetAll()
        {
            return dbSet.ToList();
        }

        public IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.Where(predicate);
        }

        public void Add(Entity entity)
        {
            dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<Entity> entities)
        {
            dbSet.AddRange(entities);
        }

        public void Remove(Entity entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<Entity> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public void Update(Entity entity)
        {
            dbSet.Update(entity);
        }
    }
}
