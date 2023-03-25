namespace Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string HomePage { get; set; } = string.Empty;

        public Customer(Guid id,
                        string name,
                        string email,
                        string address,
                        string city,
                        string region,
                        string postalCode,
                        string country,
                        string phone,
                        string homePage)
        {
            Id = id;
            Name = name;
            Email = email;
            Address = address;
            City = city;
            Region = region;
            PostalCode = postalCode;
            Country = country;
            Phone = phone;
            HomePage = homePage;
        }
    }
}
