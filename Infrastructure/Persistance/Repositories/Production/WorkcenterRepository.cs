using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class WorkcenterRepository : Repository<Workcenter, Guid>, IWorkcenterRepository
{
    public WorkcenterRepository(ApplicationDbContext context) : base(context) 
    { }

    public override async Task<IEnumerable<Workcenter>> GetAll()
    {
        return await dbSet.AsNoTracking().OrderBy(w => w.Description).ToListAsync();
    }
}