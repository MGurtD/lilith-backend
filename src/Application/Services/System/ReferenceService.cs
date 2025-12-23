using Application.Contracts;
using Application.Services;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;
using System.Text;

namespace Application.Services.System
{
    public class ReferenceService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IReferenceService
    {
        public GenericResponse CanDelete(Guid referenceId)
        {
            var sb = new StringBuilder();

            var resp = true;
            sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.Header"));

            if (unitOfWork.SalesOrderDetails.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.PurchaseOrders"));
            }
            if (unitOfWork.Receipts.Details.Find(p => p.ReferenceId.Equals(referenceId)).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.Receipts"));
            }
            if (unitOfWork.StockMovements.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.StockMovements"));
            }
            if (unitOfWork.WorkMasters.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.ProductionRoute"));
            }
            if (unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(p => p.ReferenceId == referenceId).Any())
            {
                resp = false;
                sb.AppendLine(localizationService.GetLocalizedString("Reference.Delete.BillOfMaterials"));
            }

            return new GenericResponse(resp, sb.ToString());
        }

        public async Task<List<Reference>> GetReferenceByCategory(string categoryName)
        {
            var categoryReferences = await unitOfWork.References.FindAsync(r => r.CategoryName == categoryName);
            return categoryReferences;
        }

        public async Task<GenericResponse> UpdatePriceFromReceipt(Receipt receipt)
        {
            if (receipt == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("InvoiceInvalidFormat"));
            }
            var supplierId = receipt.SupplierId;
            var format = new ReferenceFormat();
            foreach (ReceiptDetail detail in receipt.Details)
            {
                detail.Reference = await unitOfWork.References.Get(detail.ReferenceId);
                if (detail.Reference != null && detail.Reference.ReferenceFormatId.HasValue)
                {
                    format = await unitOfWork.ReferenceFormats.Get(detail.Reference.ReferenceFormatId.Value);
                }
                decimal price;
                if (detail.Reference!.CategoryName == "Material")
                {
                    if (format!.Code != "UNITATS")
                    {
                        price = detail.KilogramPrice;
                    }
                    else
                    {
                        price = detail.UnitPrice;
                    }

                }
                else
                {
                    price = detail.Amount / detail.Quantity;
                }
                detail.Reference.LastCost = price;

                var supplierreference = await unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(detail.ReferenceId, supplierId);
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
                    await unitOfWork.Suppliers.AddSupplierReference(newsupplierReference);
                }
                else
                {
                    supplierreference.SupplierPrice = price;
                    await unitOfWork.Suppliers.UpdateSupplierReference(supplierreference);
                }
                await unitOfWork.References.Update(detail.Reference);
            }
            return new GenericResponse(true);

        }
        public async Task<decimal> GetPrice(Guid referenceId, Guid? supplierId)
        {
            var supplierReference = new SupplierReference();
            if (supplierId != null)
                supplierReference = await unitOfWork.Suppliers.GetSupplierReferenceBySupplierAndId(referenceId, supplierId.Value);

            var reference = await unitOfWork.References.Get(referenceId);
            if (supplierReference != null)
                return supplierReference.SupplierPrice;
            if (reference != null)
                return reference.Price;

            return decimal.Zero;
        }
    }
}





