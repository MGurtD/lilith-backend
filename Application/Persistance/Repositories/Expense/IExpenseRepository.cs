using Domain.Entities.Expense;

namespace Application.Persistance.Repositories.Expense
{
    public interface IExpenseRepository : IRepository<Expenses, Guid>
    {
    }
}
