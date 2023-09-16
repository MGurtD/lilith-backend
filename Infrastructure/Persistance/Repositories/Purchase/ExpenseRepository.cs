using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Purchase;
using Domain.Entities;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ExpenseRepository : Repository<Expenses, Guid>, IExpenseRepository
    {
        //public IRepository<Expenses, Guid> expenseRepository { get; }
        public ExpenseRepository(ApplicationDbContext context) : base(context) 
        {

        }

    public virtual IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.AsNoTracking().Where(predicate);
    }
    /*public async Task<Expenses?> GetByType(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include(d => d.ExpenseType)
                .FirstOrDefault(e => e.ExpenseTypeId == id);
        }*/
    }
}
