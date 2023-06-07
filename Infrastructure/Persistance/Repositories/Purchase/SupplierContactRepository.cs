using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class SupplierContactRepository : Repository<SupplierContact, Guid>, ISupplierContactRepository
    {
        public SupplierContactRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
