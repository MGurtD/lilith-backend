using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterShiftRepository : Repository<WorkcenterShift, Guid>, IWorkcenterShiftRepository
    {
        public IWorkcenterShiftDetailRepository Details { get; }

        public WorkcenterShiftRepository(ApplicationDbContext context) : base(context)
        {
            Details = new WorkcenterShiftDetailRepository(context);
        }

        public IQueryable<WorkcenterShift> FindWithDetails(Expression<Func<WorkcenterShift, bool>> predicate)
        {
            return dbSet.Include(ws => ws.Details).Where(predicate);
        }
    }
}
