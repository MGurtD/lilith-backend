using Application.Persistance.Repositories;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Domain.Repositories.Purchase;

public interface IMaterialRepository : IRepository<Reference, Guid>
{
    IEnumerable<Reference> GetMaterials();

}
