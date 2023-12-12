using Application.Contracts;
using Application.Persistance;
using Application.Services;

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
            var resp = true;
            var message = "Referencia amb dependencies:\n";
            var salesOrderDetails = _unitOfWork.SalesOrderDetails.Find(p => p.ReferenceId == referenceId).Count();
            if (salesOrderDetails > 0)
            {
                resp = false;
                message = message + "- Te comandes de compra\n";
            }
            var receiptDetails = _unitOfWork.Receipts.Details.Find(p => p.ReferenceId.Equals(referenceId)).Count();
            if (receiptDetails > 0)
            {
                resp = false;
                message = message + "- Te albarans de recepció\n";
            }
            var stock = _unitOfWork.StockMovements.Find(p => p.ReferenceId == referenceId).Count();
            if (stock > 0)
            {
                resp = false;
                message = message + "- Te moviments de magatzem\n";
            }
            var workmaster = _unitOfWork.WorkMasters.Find(p => p.ReferenceId == referenceId).Count();
            if (workmaster > 0)
            {
                resp = false;
                message = message + "- Te una ruta de producció definida\n";
            }
            var bom = _unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(p => p.ReferenceId == referenceId).Count();
            if (bom > 0)
            {
                resp = false;
                message = message + "- Forma part d'una llista de materials\n";
            }

            return new GenericResponse(resp,message);
        }
    }
}
