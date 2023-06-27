using Application.Persistance.Repositories.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterTypeRepository : Repository<WorkcenterType, Guid>, IWorkcenterTypeRepository
    {
        public WorkcenterTypeRepository(ApplicationDbContext context) : base(context) 
        { }
    }
}
