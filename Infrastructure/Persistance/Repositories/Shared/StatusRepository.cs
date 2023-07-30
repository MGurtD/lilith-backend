using Application.Persistance.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class StatusRepository : Repository<Status, Guid>, IStatusRepository
    {
        public IStatusTransitionRepository TransitionRepository { get; }

        public StatusRepository(ApplicationDbContext context, IStatusTransitionRepository transitionRepository) : base(context)
        {
            TransitionRepository = transitionRepository;
        }

        public override async Task<Status?> Get(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include(d => d.Transitions)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddTransition(StatusTransition transition)
        {
            await TransitionRepository.Add(transition);
        }

        public async Task UpdateTransition(StatusTransition transition)
        {
            await TransitionRepository.Update(transition);
        }

        public async Task<bool> RemoveTransition(StatusTransition transition)
        {
            await TransitionRepository.Remove(transition);
            return true;
        }

    }
}
