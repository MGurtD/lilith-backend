﻿using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface ISupplierRepository : IRepository<Supplier, Guid>
    {
        SupplierContact? GetContactById(Guid id);
        Task AddContact(SupplierContact contact);
        Task UpdateContact(SupplierContact contact);
        Task RemoveContact(SupplierContact contact);

        Task<SupplierReference?> GetSupplierReferenceById(Guid id);
        IEnumerable<SupplierReference> GetSupplierReferences(Guid supplierReferenceId);
        IEnumerable<SupplierReference> GetReferenceSuppliers(Guid referenceId);
        Task AddSupplierReference(SupplierReference reference);
        Task UpdateSupplierReference(SupplierReference reference);
        Task RemoveSupplierReference(SupplierReference reference);
    }
}
