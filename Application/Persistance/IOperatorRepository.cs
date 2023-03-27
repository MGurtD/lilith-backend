using Domain.Entities;

namespace Application.Persistance
{
    public interface IOperatorRepository : IRepository<Operator, Guid>
    {
    }
}
