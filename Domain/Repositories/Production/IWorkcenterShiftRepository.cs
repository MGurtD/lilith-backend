using Domain.Entities.Production;
using System.Linq.Expressions;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkcenterShiftRepository : IRepository<WorkcenterShift, Guid>
    {
        IWorkcenterShiftDetailRepository Details { get; }

        Task<WorkcenterShift?> GetCurrentWorkcenterShiftWithCurrentDetails(Guid workcenterId);

        IQueryable<WorkcenterShift> FindWithDetails(Expression<Func<WorkcenterShift, bool>> predicate);
    }
}
