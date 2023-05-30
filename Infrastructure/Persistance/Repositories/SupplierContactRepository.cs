using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class SupplierContactRepository : Repository<SupplierContact, Guid>, ISupplierContactRepository
    {
        public SupplierContactRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
