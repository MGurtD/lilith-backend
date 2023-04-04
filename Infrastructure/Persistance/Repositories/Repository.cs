using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

using Application.Persistance;

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

        public async Task<Entity?> Get(Id id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Entity>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        public IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.Where(predicate);
        }

        public async Task Add(Entity entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddRange(IEnumerable<Entity> entities)
        {
            await dbSet.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public async Task Update(Entity entity)
        {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task Remove(Entity entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task RemoveRange(IEnumerable<Entity> entities)
        {
            dbSet.RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        
    }
}
