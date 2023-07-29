using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Purchase;
using Application.Persistance.Repositories.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ExpenseTypeRepository : Repository<ExpenseType, Guid>, IExpenseTypeRepository
    {
        public ExpenseTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
