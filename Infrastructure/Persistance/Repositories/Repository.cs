using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Application.Persistance.Repositories;

namespace Infrastructure.Persistance.Repositories
{
    public class Repository<Entity, Id> : IRepository<Entity, Id> where Entity : Domain.Entities.Entity
    {
        protected ApplicationDbContext context;
        internal DbSet<Entity> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<Entity>();
        }

        public virtual async Task<Entity?> Get(Id id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<Entity>> GetAll()
        {
            return await dbSet.AsNoTracking().ToListAsync();
        }

        public virtual IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.AsNoTracking().Where(predicate);
        }

        public virtual async Task<bool> Exists(Guid Id)
        {
            return await dbSet.AnyAsync(e => e.Id == Id);
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
            entity.UpdatedOn = DateTime.Now;

            dbSet.Update(entity); 
            await context.SaveChangesAsync();
        }

        public bool UpdateWithoutSave(Entity entity)
        {
            entity.UpdatedOn = DateTime.Now;

            dbSet.Update(entity);
            return true;
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
