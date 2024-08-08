using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Shared;
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
    }
}
