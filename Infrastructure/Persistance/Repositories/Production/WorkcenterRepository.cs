using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Application.Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterRepository : Repository<Workcenter, Guid>, IWorkcenterRepository
    {
        public WorkcenterRepository(ApplicationDbContext context) : base(context) 
        { }

        public override async Task<IEnumerable<Workcenter>> GetAll()
        {
            
            return dbSet.AsNoTracking().OrderBy(w => w.Description);
        }
    }
}
