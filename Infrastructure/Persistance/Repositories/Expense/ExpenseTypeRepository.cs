using Application.Persistance.Repositories.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Expense;

namespace Infrastructure.Persistance.Repositories.Expense
{
    public class ExpenseTypeRepository : Repository<ExpenseType, Guid>, IExpenseTypeRepository
    {
        public ExpenseTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
