using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class StatusTransitionRepository : Repository<StatusTransition, Guid>, IStatusTransitionRepository
    {
        public StatusTransitionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
