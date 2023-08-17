﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

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

        public bool IsValidForSales()
        {
            var invalid = string.IsNullOrWhiteSpace(TaxName) || string.IsNullOrWhiteSpace(VatNumber) || string.IsNullOrWhiteSpace(AccountNumber);
            return !invalid;
        }

        public CustomerAddress? MainAddress()
        {
            if (Address.Any())
            {
                if (Address.Any(c => c.Main)) return Address.FirstOrDefault(c => c.Main);
                return Address.FirstOrDefault();
            }
            return null;
        }

        public CustomerContact? MainContact()
        {
            if (Contacts.Any())
            {
                if (Contacts.Any(c => c.Main)) return Contacts.FirstOrDefault(c => c.Main);
                return Contacts.FirstOrDefault();
            }
            return null;
        }

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid? PaymentMethodId { get; set; }

        public CustomerType? Type { get; set; }
        public Guid CustomerTypeId { get; set; }


        public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
        public ICollection<CustomerAddress> Address { get; set; } = new List<CustomerAddress>();
    }
}
