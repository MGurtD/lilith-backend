﻿using Application.Persistance.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class LifecycleRepository : Repository<Lifecycle, Guid>, ILifecycleRepository
    {
        public IStatusRepository StatusRepository { get; }

        public LifecycleRepository(ApplicationDbContext context) : base(context)
        {
            StatusRepository = new StatusRepository(context);
        }

        public override async Task<IEnumerable<Lifecycle>> GetAll()
        {
            return await dbSet
                .AsNoTracking()
                .Include("Statuses")
                .ToListAsync();
        }

        public override async Task<Lifecycle?> Get(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include("Statuses.Transitions")
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Lifecycle?> GetByName(string name)
        {
            return await dbSet
                .AsNoTracking()
                .Include("Statuses.Transitions")
                .FirstOrDefaultAsync(e => e.Name == name);
        }

        public async Task<Status?> GetStatusByName(string lifecycleName, string name)
        {
            var lifecycle = await GetByName(lifecycleName);
            if (lifecycle == null || lifecycle.Statuses == null) return null;

            return lifecycle.Statuses.FirstOrDefault(s => s.Name == name);
        }
    }
}
