namespace Domain.Entities.Sales
{
    public class Customer : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string ComercialName { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public string Web { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid? PaymentMethodId { get; set; }

        public CustomerType? Type { get; set; }
        public Guid CustomerTypeId { get; set; }


        public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
        public ICollection<CustomerAddress> Address { get; set; } = new List<CustomerAddress>();
    }
}
