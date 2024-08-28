using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseOrderRepository : Repository<PurchaseOrder, Guid>, IPurchaseOrderRepository
    {
        public IRepository<PurchaseOrderDetail, Guid> Details { get; }

        public IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; }

        public PurchaseOrderRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<PurchaseOrderDetail, Guid>(context);
            Receptions = new Repository<PurchaseOrderReceiptDetail, Guid>(context);
        }

        public override async Task<PurchaseOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<PurchaseOrder>> GetOrdersWithDetailsToReceiptBySupplier(Guid supplierId)
        {
            return
                await dbSet
                    .Include(d => d.Details)
                        .ThenInclude(d => d.Reference)
                    .Include(d => d.Details)
                        .ThenInclude(d => d.WorkOrderPhase)
                        .ThenInclude(d => d.WorkOrder)
                .Where(e => e.SupplierId == supplierId && e.Details.Any(d => d.ReceivedQuantity < d.Quantity))
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id)
        {
            var order = await Get(id);
            if (order == null)
                return [];

            var detailIds = order.Details.Select(d => d.Id).ToList();
            return await Receptions
                        .FindAsync(d => detailIds.Contains(d.PurchaseOrderDetailId));
        }
    }
}
