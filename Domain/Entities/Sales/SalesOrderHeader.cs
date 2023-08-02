using Domain.Entities.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Sales
{
    public  class SalesOrderHeader : Entity
    {
        public Customer? Customer { get; set; }
        public Guid? CustomerId { get; set; }
        public Exercise? Exercise { get; set; }
        public Guid? ExerciseId { get; set; }
        public DateTime SalesOrderDate { get; set; }
        public int SalesOrderNumber { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerComercialName { get; set; } = string.Empty;
        public string CustomerTaxName { get; set; } = string.Empty; 
        public string CustomerVatNumber { get; set; } = string.Empty;
        public string CustomerAccountNumber { get; set; } = string.Empty;
        public Site? Site { get; set; }
        public Guid? SiteId { get; set;}
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();


    }
}
