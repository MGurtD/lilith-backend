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

        public async Task<GenericResponse> CanDelete(Guid referenceId)
        {
            var resp = true;
            var message = "";
            var salesOrderDetails = _unitOfWork.SalesOrderDetails.Find(p => p.ReferenceId == referenceId);
            if (salesOrderDetails != null)
            {
                resp = false;
                message = message + " La referencia amb ID:" + referenceId + " te comandes de compra";
            }
            var receiptDetails = _unitOfWork.Receipts.Details.Find(p => p.ReferenceId.Equals(referenceId));
            if (receiptDetails != null)
            {
                resp = false;
                message = message + " La referencia amb ID:" + referenceId + " te albarans de recepció";
            }
            var stock = _unitOfWork.StockMovements.Find(p => p.ReferenceId == referenceId);
            if (stock != null)
            {
                resp = false;
                message = message + " La referencia amb ID:" + referenceId + " te moviments de magatzem";
            }
            var workmaster = _unitOfWork.WorkMasters.Find(p => p.ReferenceId == referenceId);
            if (workmaster != null)
            {
                resp = false;
                message = message + " La referencia amb ID:" + referenceId + " te una ruta de producció definida";
            }
            var bom = _unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(p => p.ReferenceId == referenceId);
            if (bom != null)
            {
                resp = false;
                message = message + " La referencia amb ID:" + referenceId + " forma part d'una llista de materials";
            }

            return new GenericResponse(resp, message);
        }
    }
}
