using Domain.Entities;

namespace Application.Persistance
{
    public interface ISupplierRepository : IRepository<Supplier, Guid>
    {
        SupplierContact? GetContactById(Guid id);
        Task AddContact(SupplierContact contact);
        Task UpdateContact(SupplierContact contact);
        Task RemoveContact(SupplierContact contact);
    }
}
