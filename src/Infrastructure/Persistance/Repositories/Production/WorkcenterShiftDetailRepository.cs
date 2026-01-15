using Application.Contracts;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterShiftDetailRepository : Repository<WorkcenterShiftDetail, Guid>, IWorkcenterShiftDetailRepository
    {

        public WorkcenterShiftDetailRepository(ApplicationDbContext context) : base(context)
        { }
    }
}
