using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ReceiptRepository : Repository<Receipt, Guid>, IReceiptRepository
    {
        public IRepository<ReceiptDetail, Guid> Details { get; }

        public ReceiptRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<ReceiptDetail, Guid>(context);
        }

        public override async Task<Receipt?> Get(Guid id)
        {
            return await dbSet
                        .Include("Details.Reference")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
