using Application.Persistance;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistance.Repositories
{
    public class OperatorRepository : Repository<Operator, Guid>, IOperatorRepository
    {
        public OperatorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
