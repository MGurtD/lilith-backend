using Application.Contracts;
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

    public async Task<IEnumerable<Workcenter>> GetVisibleInPlant()
    {
        return await dbSet
            .Include(w => w.Area)
            .Include(w => w.WorkcenterType)
            .Include(w => w.Shift)
            .Where(w => !w.Disabled && w.Area != null && w.Area.IsVisibleInPlant)
            .OrderBy(w => w.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}