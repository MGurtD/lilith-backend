using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Customer : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string ComercialName { get; set; } = string.Empty;
        public string TaxName { get; set;} = string.Empty;
        public string VatNumber { get; set; } = string.Empty;

        public string Web { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;

        public CustomerType? Type { get; set; }
        public Guid CustomerTypeId { get; set; }

        public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();   
        public ICollection<CustomerAddress> Address { get; set; } = new List<CustomerAddress>();
    }
}
