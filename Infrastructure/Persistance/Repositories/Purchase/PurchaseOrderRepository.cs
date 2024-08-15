using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseOrderRepository : Repository<PurchaseOrder, Guid>, IPurchaseOrderRepository
    {
        public IRepository<PurchaseOrderDetail, Guid> Details { get; }

        public PurchaseOrderRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<PurchaseOrderDetail, Guid>(context);
        }

        public override async Task<PurchaseOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
