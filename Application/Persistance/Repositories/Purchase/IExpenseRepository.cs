using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IExpenseRepository : IRepository<Expenses, Guid>
    {
    }
}
