using Application.Persistance.Repositories.Expense;
using Domain.Entities.Expense;

namespace Infrastructure.Persistance.Repositories.Expense
{
    public class ExpenseRepository : Repository<Expenses, Guid>, IExpenseRepository
    {

        public ExpenseRepository(ApplicationDbContext context) : base(context) 
        { 
        }
    }
}

