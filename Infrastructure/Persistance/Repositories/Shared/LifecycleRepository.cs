﻿using Application.Persistance.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class LifecycleRepository : Repository<Lifecycle, Guid>, ILifeCycleRepository
    {
        public IStatusRepository StatusRepository { get; }

        public LifecycleRepository(ApplicationDbContext context, IStatusRepository statusRepository) : base(context)
        {
            StatusRepository = statusRepository;
        }

        public override async Task<Lifecycle?> Get(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include("Statuses.Transitions")
                .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
