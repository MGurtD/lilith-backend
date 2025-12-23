using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterShiftRepository(ApplicationDbContext context) : Repository<WorkcenterShift, Guid>(context), IWorkcenterShiftRepository
    {
        public IWorkcenterShiftDetailRepository Details { get; } = new WorkcenterShiftDetailRepository(context);

        public IQueryable<WorkcenterShift> FindWithDetails(Expression<Func<WorkcenterShift, bool>> predicate)
        {
            return dbSet.Include(ws => ws.Details).Where(predicate);
        }

        public async Task<WorkcenterShift?> GetCurrentWorkcenterShiftWithCurrentDetails(Guid workcenterId)
        {
            var currentWorkcenterShift = await FindWithDetails(ws => ws.Current && ws.WorkcenterId == workcenterId).FirstOrDefaultAsync();
            if (currentWorkcenterShift == null) return null;

            currentWorkcenterShift.Details = currentWorkcenterShift.Details.Where(d => d.Current).ToList();
            return currentWorkcenterShift;
        }
    }
}
