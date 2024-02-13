using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services;
using Application.Services.Purchase;
using Application.Services.Warehouse;
using Domain.Entities.Purchase;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Api.Services.Purchase
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockMovementService _stockMovementService;
        private readonly IExerciseService _exerciseService;

        public ReceiptService(IUnitOfWork unitOfWork, IStockMovementService stockMovementService, IExerciseService exerciseService)
        {
            _unitOfWork = unitOfWork;
            _stockMovementService = stockMovementService;
            _exerciseService = exerciseService;
        }

        public IEnumerable<Receipt> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var receipts = _unitOfWork.Receipts.Find(p => p.Date >= startDate && p.Date <= endDate);
            return receipts;
        }

        public IEnumerable<Receipt> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Receipt> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid supplierId)
        {
            var receipts = _unitOfWork.Receipts.Find(p => p.Date >= startDate && p.Date <= endDate && p.SupplierId == supplierId);
            return receipts;
        }

        public IEnumerable<Receipt> GetByInvoice(Guid invoiceId)
        {
            var receipts = _unitOfWork.Receipts.Find(p => p.PurchaseInvoiceId == invoiceId);
            return receipts;
        }

        public IEnumerable<Receipt> GetByStatus(Guid statusId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Receipt> GetBySupplier(Guid supplierId, bool withoutInvoice)
        {
            var receipts = _unitOfWork.Receipts.Find(p => p.SupplierId == supplierId);
            receipts = withoutInvoice ? receipts.Where(r => r.PurchaseInvoiceId == null) : receipts;
            return receipts;
        }

        public async Task<GenericResponse> Create(CreateReceiptRequest createRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, new List<string>() { "Exercici inexistent" });

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == "Receipts").FirstOrDefault();
            if (status == null) return new GenericResponse(false, new List<string>() { "Cicle de vida 'Receipts' inexistent" });
            if (!status.InitialStatusId.HasValue) return new GenericResponse(false, new List<string>() { "El cicle de vida 'Receipts' no té un estat inicial" });

            //var receiptCounter = exercise.ReceiptCounter == 0 ? 1 : exercise.ReceiptCounter;
            var receiptCounterObj = await _exerciseService.GetNextCounter(exercise.Id, "receipt");
            if (receiptCounterObj == null) return new GenericResponse(false, new List<string>() { "Error al crear el comptador" });

            var receiptCounter = receiptCounterObj.Content.ToString();
            var receipt = new Receipt()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                SupplierId = createRequest.SupplierId,
                Date = createRequest.Date,
                Number = receiptCounter,
                StatusId = status.InitialStatusId.Value
            };
            await _unitOfWork.Receipts.Add(receipt);

            return new GenericResponse(true);
        }
        public async Task<GenericResponse> Update(Receipt receipt)
        {
            // Netejar dependencies per evitar col·lisions de EF
            receipt.Details?.Clear();

            var exists = await _unitOfWork.Receipts.Exists(receipt.Id);
            if (!exists) return new GenericResponse(false, new List<string>() { $"Id {receipt.Id} inexistent" });

            await _unitOfWork.Receipts.Update(receipt);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var receipt = await _unitOfWork.Receipts.Get(id);
            if (receipt == null) return new GenericResponse(false, new List<string>() { $"Id {id} inexistent" });

            await _unitOfWork.Receipts.Remove(receipt);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> CalculateDetailWeightAndPrice(ReceiptDetail detail)
        {
            detail.Reference ??= await _unitOfWork.References.Get(detail.ReferenceId);
            if (detail.Reference is null) return new GenericResponse(false, "Referencia no existent");
            if (!detail.Reference.ReferenceFormatId.HasValue) return new GenericResponse(false, "Referencia sense format");
            if (!detail.Reference.ReferenceTypeId.HasValue) return new GenericResponse(false, "Referencia sense tipus");
            var referenceType = await _unitOfWork.ReferenceTypes.Get(detail.Reference.ReferenceTypeId.Value);

            // Obtenir calculadora segons el format
            var format = await _unitOfWork.ReferenceFormats.Get(detail.Reference.ReferenceFormatId.Value);
            var dimensionsCalculator = ReferenceFormatCalculationFactory.Create(format!.Code);
            // Assignar dimensions a la calculadoras
            var dimensions = new ReferenceDimensions();
            dimensions.SetFromReceiptDetail(detail);
            dimensions.Density = referenceType!.Density;

            // Calcular el pes
            detail.UnitWeight = Math.Round(dimensionsCalculator.Calculate(dimensions), 2);
            detail.TotalWeight = detail.UnitWeight * detail.Quantity;
            // Calcular el preu
            detail.UnitPrice = detail.KilogramPrice * detail.UnitWeight;
            detail.Amount = detail.KilogramPrice * detail.TotalWeight;

            return new GenericResponse(true, detail);
        }

        public async Task<GenericResponse> AddDetail(ReceiptDetail detail)
        {
            // var calculationResponse = await CalculateDetailWeightAndPrice(detail);
            // if (calculationResponse.Result) detail = (ReceiptDetail)calculationResponse.Content!;
            detail.Reference = null;

            await _unitOfWork.Receipts.Details.Add(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateDetail(ReceiptDetail detail)
        {
            var exists = await _unitOfWork.Receipts.Details.Exists(detail.Id);
            if (!exists) return new GenericResponse(false, $"Id {detail.Id} inexistent");

            // var calculationResponse = await CalculateDetailWeightAndPrice(detail);
            // if (calculationResponse.Result) detail = (ReceiptDetail) calculationResponse.Content!;

            detail.Reference = null;
            await _unitOfWork.Receipts.Details.Update(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = await _unitOfWork.Receipts.Details.Get(id);
            if (detail == null) return new GenericResponse(false, $"Id {id} inexistent");

            await _unitOfWork.Receipts.Details.Remove(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> MoveToWarehose(Receipt receipt)
        {
            var warehouse = _unitOfWork.Warehouses.Find(w => w.Disabled == false).FirstOrDefault();
            if (warehouse == null) return new GenericResponse(false, "No existeixen magatzems actius");
            if (warehouse.DefaultLocationId == null) return new GenericResponse(false, "No hi ha una ubicació per defecte");

            var detailsToMove = receipt.Details!.Where(d => d.StockMovementId == null);
            foreach (var detail in detailsToMove)
            {
                var stockMovement = new StockMovement
                {
                    LocationId = warehouse.DefaultLocationId,
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.INPUT,
                    Description = $"Albará {receipt.Number}"
                };
                stockMovement.SetFromReceiptDetail(detail);
                detail.Reference = null;
                detail.StockMovementId = stockMovement.Id;

                await _stockMovementService.Create(stockMovement);
                await _unitOfWork.Receipts.Details.Update(detail);
            }

            return new GenericResponse(true, detailsToMove);
        }

        public async Task<GenericResponse> RetriveFromWarehose(Receipt receipt)
        {
            var detailsToRetrive = receipt.Details!.Where(d => d.StockMovementId != null);
            foreach (var detail in detailsToRetrive)
            {
                Guid oldStockMovementId = detail.StockMovementId!.Value;

                detail.Reference = null;
                detail.StockMovementId = null;
                await _unitOfWork.Receipts.Details.Update(detail);
                await _stockMovementService.Remove(oldStockMovementId);
            }

            return new GenericResponse(true, detailsToRetrive);
        }
        
    }
}