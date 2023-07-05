using Domain.Entities.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance.Repositories.Expense
{
    public interface IExpenseTypeRepository : IRepository<ExpenseType, Guid>
    {
    }
}
