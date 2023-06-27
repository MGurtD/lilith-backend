using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
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
    }
}
