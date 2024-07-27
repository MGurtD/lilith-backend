using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class SupplierRepository : Repository<Supplier, Guid>, ISupplierRepository
    {
        private readonly IRepository<SupplierContact, Guid> _supplierContactRepository;
        private readonly IRepository<SupplierReference, Guid> _supplierReferenceRepository;

        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
            _supplierContactRepository = new Repository<SupplierContact, Guid>(context);
            _supplierReferenceRepository = new Repository<SupplierReference, Guid>(context);
        }

        public override async Task<Supplier?> Get(Guid id)
        {
            return await dbSet.Include(s => s.Contacts).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Supplier>> GetAll()
        {
            return await dbSet.Include(s => s.Contacts).AsNoTracking().ToListAsync();
        }

        public SupplierContact? GetContactById(Guid id)
        {
            var contact = _supplierContactRepository.Find(c => c.Id == id).FirstOrDefault();
            return contact;
        }

        public async Task AddContact(SupplierContact contact)
        {
            await _supplierContactRepository.Add(contact);
        }

        public async Task RemoveContact(SupplierContact contact)
        {
            await _supplierContactRepository.Remove(contact);
        }

        public async Task UpdateContact(SupplierContact contact)
        {
            await _supplierContactRepository.Update(contact);
        }

        public IEnumerable<SupplierReference> GetSupplierReferences(Guid id)
        {
            return _supplierReferenceRepository.Find(r => r.SupplierId == id);
        }

        public async Task<SupplierReference?> GetSupplierReferenceById(Guid supplierReferenceId)
        {
            return await _supplierReferenceRepository.Get(supplierReferenceId);
        }

        public async Task AddSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Add(reference);
        }

        public async Task UpdateSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Update(reference);
        }

        public async Task RemoveSupplierReference(SupplierReference reference)
        {
            await _supplierReferenceRepository.Remove(reference);
        }

        public IEnumerable<SupplierReference> GetReferenceSuppliers(Guid referenceId)
        {
            var suppliersReference = _supplierReferenceRepository.Find(r => r.ReferenceId == referenceId);
            var suppliers = dbSet.ToList();
            
            foreach (var supplierReference in suppliersReference)
                supplierReference.Supplier = suppliers.FirstOrDefault(s => s.Id == supplierReference.SupplierId);

            return suppliersReference;
        }
    }
}
