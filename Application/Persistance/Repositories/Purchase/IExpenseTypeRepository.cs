using Domain.Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IExpenseTypeRepository : IRepository<ExpenseType, Guid>
    {
    }
}
