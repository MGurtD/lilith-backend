using Application.Persistance.Repositories.Sales;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesOrderHeaderRepository : Repository<SalesOrderHeader, Guid>, ISalesOrderHeaderRepository
    {
        private readonly ISalesOrderDetailRepository _salesOrderDetailRepository;
        public SalesOrderHeaderRepository(ApplicationDbContext context, ISalesOrderDetailRepository salesOrderDetailRepository) : base(context)
        {
            _salesOrderDetailRepository = salesOrderDetailRepository;
        }
        public override async Task<SalesOrderHeader?> Get(Guid id)
        {
            return await dbSet.Include(s => s.SalesOrderDetails).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }
        public SalesOrderDetail? GetDetailById(Guid id)
        {
            var salesOrderDetail = _salesOrderDetailRepository.Find(c => c.Id == id).FirstOrDefault();
            return salesOrderDetail;
        }
        public async Task AddDetail(SalesOrderDetail detail)
        {
            await _salesOrderDetailRepository.Add(detail);
        }
        public async Task UpdateDetail(SalesOrderDetail detail)
        {
            await _salesOrderDetailRepository.Update(detail);
        }
        public async Task RemoveDetail(SalesOrderDetail detail)
        {
            await _salesOrderDetailRepository.Remove(detail);
        }
    }
}
