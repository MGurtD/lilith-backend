using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ExpenseRepository : Repository<Expenses, Guid>, IExpenseRepository
    {

        public ExpenseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

