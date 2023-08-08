using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public  class SalesInvoice : Entity
    {
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }

        public Customer? Customer { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerComercialName { get; set; } = string.Empty;
        public string CustomerTaxName { get; set; } = string.Empty; 
        public string CustomerVatNumber { get; set; } = string.Empty;
        public string CustomerAccountNumber { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerRegion { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;

        public Site? Site { get; set; }
        public Guid? SiteId { get; set;}
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
                
        public Exercise? Exercise { get; set; }
        public Guid? ExerciseId { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    }
}
