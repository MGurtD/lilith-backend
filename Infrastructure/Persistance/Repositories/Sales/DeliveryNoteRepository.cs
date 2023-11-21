using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Sales;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class DeliveryNoteRepository : Repository<DeliveryNote, Guid>, IDeliveryNoteRepository
    {
        public IRepository<DeliveryNoteDetail, Guid> Details { get; }

        public DeliveryNoteRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<DeliveryNoteDetail, Guid>(context);
        }

        public override async Task<DeliveryNote?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                        .Include(d => d.SalesInvoice)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
