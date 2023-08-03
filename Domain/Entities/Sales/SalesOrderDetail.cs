using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Sales
{
    public class SalesOrderDetail : Entity
    {
        public Guid SalesOrderHeaderId { get; set; }
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime EstimatedDeliveryDate { get; set; }
        public Boolean IsServed { get; set; }
        public Boolean IsInvoiced { get; set; }

    }
}
