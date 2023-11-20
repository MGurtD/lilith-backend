﻿using Domain.Entities.Production;

namespace Domain.Entities.Sales
{
    public  class SalesOrderHeader : Entity
    {
        public Exercise? Exercise { get; set; }
        public Guid? ExerciseId { get; set; }
        public DateTime SalesOrderDate { get; set; }
        public int SalesOrderNumber { get; set; }

        public Customer? Customer { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerSalesOrderNumber { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerComercialName { get; set; } = string.Empty;
        public string CustomerTaxName { get; set; } = string.Empty; 
        public string CustomerVatNumber { get; set; } = string.Empty;
        public string CustomerAccountNumber { get; set; } = string.Empty;
        public void SetCustomer(Customer customer)
        {
            CustomerId = customer.Id;
            CustomerAccountNumber = customer.AccountNumber;
            CustomerComercialName = customer.ComercialName;
            CustomerTaxName = customer.TaxName;
            CustomerVatNumber = customer.VatNumber;
        }

        public Site? Site { get; set; }
        public Guid? SiteId { get; set;}
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public void SetSite(Site site)
        {
            SiteId = site.Id;
            Name = site.Name;
            Address = site.Address;
            City = site.City;
            PostalCode = site.PostalCode;
            Region = site.Region;
            Country = site.Country;
            VatNumber = site.VatNumber;
        }

        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }

        public Guid? DeliveryNoteId { get; set; }
        public DeliveryNote? DeliveryNote { get; set; }

        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

        public void Deliver()
        {
            if (SalesOrderDetails != null)
                foreach (var item in SalesOrderDetails)
                    item.IsDelivered = true;
        }
        public void UnDeliver()
        {
            if (SalesOrderDetails != null)
                foreach (var item in SalesOrderDetails)
                    item.IsDelivered = false;
        }

    }
}
