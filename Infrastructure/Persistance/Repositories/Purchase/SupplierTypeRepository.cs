using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class SupplierTypeRepository : Repository<SupplierType, Guid>, ISupplierTypeRepository
    {
        public SupplierTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
