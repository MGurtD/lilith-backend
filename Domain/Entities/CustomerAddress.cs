using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class CustomerAddress : Entity
    {
        public string Name { get; set; } = string.Empty;
        public bool MainAddress { get; set; } = false;
        public string Address { get; set; } = string.Empty;
        public string AddressExtraInfo { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
    }
}
