using Application.Contracts;
using Application.Persistance;
using Application.Persistance.Repositories;
using Application.Services;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;
using Infrastructure.Persistance;
using System.Text;

namespace Api.Services
{
    public class ReferenceService : IReferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        

        public ReferenceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;        
        }

        public GenericResponse CanDelete(Guid referenceId)
        {
            var sb = new StringBuilder();

            var resp = true;
            sb.AppendLine("Referència amb dependencies: ");

            if (_unitOfWork.SalesOrderDetails.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine("- Té comandes de compra");
            }
            if (_unitOfWork.Receipts.Details.Find(p => p.ReferenceId.Equals(referenceId)).Any())
            {
                resp = false;
                sb.AppendLine("- Té albarans de recepció");
            }
            if (_unitOfWork.StockMovements.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine("- Té moviments de magatzem");
            }
            if (_unitOfWork.WorkMasters.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine("- Té una ruta de producció definida");
            }
            if (_unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine("- Forma part d'una llista de materials");
            }

            return new GenericResponse(resp, sb.ToString());
        }

        public async Task<List<Reference>> GetReferenceByCategory(string categoryName)
        {
            var categoryReferences = await _unitOfWork.References.FindAsync(r => r.CategoryName == categoryName);
            return categoryReferences;
        }

        public async Task<GenericResponse> UpdatePriceFromReceipt(Receipt receipt)
        {
            if (receipt == null)
            {
                return new GenericResponse(false, "Albará mal format o incorrecte");
            }
            var supplierId = receipt.SupplierId;
            var price = Decimal.Zero;
            foreach (ReceiptDetail detail in receipt.Details)
            {
                detail.Reference = await _unitOfWork.References.Get(detail.ReferenceId);
                if (detail.Reference.CategoryName == "Material")
                {
                    price = detail.KilogramPrice;
                }
                else
                {
                    price = detail.Amount / detail.Quantity;
                }
                detail.Reference.LastCost = price;

                var supplierreference = await _unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(detail.ReferenceId, supplierId);
                if (supplierreference == null)
                {
                                        
                    var newsupplierReference = new SupplierReference()
                    {
                        Id = Guid.NewGuid(),
                        ReferenceId = detail.ReferenceId,
                        SupplierId = supplierId,
                        SupplierCode = detail.Reference.Code,
                        SupplierDescription = detail.Reference.Description,
                        SupplierPrice = price,
                        SupplyDays = 0
                    };
                    await _unitOfWork.Suppliers.AddSupplierReference(newsupplierReference);
                }
                else
                {
                    supplierreference.SupplierPrice = price;
                    await _unitOfWork.Suppliers.UpdateSupplierReference(supplierreference);
                }
                await _unitOfWork.References.Update(detail.Reference);
            }
            return new GenericResponse(true);

        }
        public async Task<decimal> GetPrice(Guid referenceId, Guid? supplierId)
        {
            var reference = new Reference();
            var supplierReference = new SupplierReference();
            if (supplierId != null)
            {
                supplierReference = await _unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(referenceId, supplierId.Value);
                
            }
            reference = await _unitOfWork.References.Get(referenceId);


            if (supplierReference == null)
            {
                return reference.Price;
            }
                return supplierReference.SupplierPrice;
        }
    }
}
