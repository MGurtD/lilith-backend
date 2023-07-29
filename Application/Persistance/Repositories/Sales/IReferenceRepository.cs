using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance.Repositories.Sales
{
    public interface IReferenceRepository : IRepository<Reference, Guid>
    {
    }
}
