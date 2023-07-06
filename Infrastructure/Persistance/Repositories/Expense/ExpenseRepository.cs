using Application.Persistance.Repositories.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Expense;

namespace Infrastructure.Persistance.Repositories.Expense
{
    public class ExpenseRepository : Repository<Expenses, Guid>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context) { }
    }
}

