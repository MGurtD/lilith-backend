using Domain.Entities.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkcenterTypeRepository : IRepository<WorkcenterType, Guid>
    {
    }
}
