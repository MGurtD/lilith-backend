using Application.Persistance;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        private readonly ICustomerContactRepository _customerContactRepository;
        private readonly ICustomerAddressRepository _customerAddressRepository;

        public CustomerRepository(ApplicationDbContext context, ICustomerContactRepository customerContactRepository, ICustomerAddressRepository customerAddressRepository) : base(context) 
        { 
            _customerContactRepository = customerContactRepository;
            _customerAddressRepository = customerAddressRepository;
        }

        public override async Task<Customer?> Get(Guid id)
        {
            return await dbSet.Include(s => s.Contacts).Include(s => s.Address).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);                                
        }

        public CustomerContact? GetContactById(Guid id) 
        { 
            var contact = _customerContactRepository.Find(c => c.Id == id).FirstOrDefault();
            return contact;
        }

        public async Task AddContact(CustomerContact contact)
        {
            await _customerContactRepository.Add(contact);
        }

        public async Task RemoveContact(CustomerContact contact)
        {
            await _customerContactRepository.Remove(contact);
        }

        public async Task UpdateContact(CustomerContact contact)
        {
            await _customerContactRepository.Update(contact);
        }

        public CustomerAddress? GetAddressById(Guid id) 
        { 
            var address = _customerAddressRepository.Find(c => c.Id==id).FirstOrDefault();
            return address;
        }

        public async Task AddAddress(CustomerAddress address) 
        { 
            await _customerAddressRepository.Add(address);
        }

        public async Task RemoveAddress(CustomerAddress address)
        {
            await _customerAddressRepository.Remove(address);
        }

        public async Task UpdateAddress(CustomerAddress address)
        {
            await _customerAddressRepository.Update(address);
        }
    }
}
