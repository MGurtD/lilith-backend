using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class TaxRepository : Repository<Tax, Guid>, ITaxRepository
    {
        public TaxRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
