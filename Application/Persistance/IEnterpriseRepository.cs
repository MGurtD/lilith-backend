using Domain.Entities;

namespace Application.Persistance
{
    public interface IEnterpriseRepository : IRepository<Enterprise, Guid>
    {
    }
}
