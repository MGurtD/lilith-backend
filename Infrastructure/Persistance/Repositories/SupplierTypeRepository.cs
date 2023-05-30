﻿using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class SupplierTypeRepository : Repository<SupplierType, Guid>, ISupplierTypeRepository
    {
        public SupplierTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
