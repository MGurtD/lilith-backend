using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Domain.Entities.Purchase;
using Domain.Entities.Warehouse;

namespace Application.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockMovementService _stockMovementService;

        public ReceiptService(IUnitOfWork unitOfWork, IStockMovementService stockMovementService)
        {
            _unitOfWork = unitOfWork;
            _stockMovementService = stockMovementService;
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

        public IEnumerable<Receipt> GetByStatus(Guid statusId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Receipt> GetBySupplier(Guid supplierId)
        {
            var receipts = _unitOfWork.Receipts.Find(p => p.SupplierId == supplierId);
            return receipts;
        }

        public async Task<GenericResponse> Create(CreateReceiptRequest createRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, new List<string>() { "Exercici inexistent" });

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == "Receipts").FirstOrDefault();
            if (status == null) return new GenericResponse(false, new List<string>() { "Cicle de vida 'Receipts' inexistent" });
            if (!status.InitialStatusId.HasValue) return new GenericResponse(false, new List<string>() { "El cicle de vida 'Receipts' no té un estat inicial" });

            var receiptCounter = exercise.ReceiptCounter == 0 ? 1 : exercise.ReceiptCounter;
            var receipt = new Receipt()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                SupplierId = createRequest.SupplierId,
                Date = createRequest.Date,
                Number = $"{exercise.Name}-{receiptCounter.ToString().PadLeft(3, '0')}",
                StatusId = status.InitialStatusId.Value
            };
            await _unitOfWork.Receipts.Add(receipt);

            exercise.ReceiptCounter = receiptCounter + 1;
            await _unitOfWork.Exercices.Update(exercise);

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
        public async Task<GenericResponse> AddDetail(ReceiptDetail detail)
        {
            await _unitOfWork.Receipts.Details.Add(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateDetail(ReceiptDetail detail)
        {
            var exists = await _unitOfWork.Receipts.Details.Exists(detail.Id);
            if (!exists) return new GenericResponse(false, new List<string>() { $"Id {detail.Id} inexistent" });

            detail.Reference = null;
            await _unitOfWork.Receipts.Details.Update(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = await _unitOfWork.Receipts.Details.Get(id);
            if (detail == null) return new GenericResponse(false, new List<string>() { $"Id {id} inexistent" });

            await _unitOfWork.Receipts.Details.Remove(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> MoveToWarehose(Receipt receipt)
        {
            var detailsToMove = receipt.Details!.Where(d => d.StockMovementId == null);
            foreach (var detail in detailsToMove) 
            {
                var stockMovement = new StockMovement
                {
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.INPUT,
                    Description = $"Albará {receipt.Number}"
                };
                stockMovement.SetFromReceiptDetail(detail);

                await _stockMovementService.Create(stockMovement);
            }

            await ChangeStatus("Recepcionat", receipt);

            return new GenericResponse(true, detailsToMove);
        }

        private async Task<GenericResponse> ChangeStatus(string StatusName, Receipt receipt)
        {
            var lifecycle = await _unitOfWork.Lifecycles.GetByName("Receipts");
            if (lifecycle == null) return new GenericResponse(false, new List<string> { $"Cicle de vida 'Receipts' inexistent" });

            var status = lifecycle.Statuses!.FirstOrDefault(s => s.Name == StatusName);
            if (status == null) return new GenericResponse(false, new List<string> { $"Estat {StatusName} inexistent al cicle de vida 'Receipts'" });
                
            receipt.StatusId = status.Id;
            await _unitOfWork.Receipts.Update(receipt);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RetriveFromWarehose(Receipt receipt)
        {
            var stockMovementIdsToRetrive = 
                from detail in receipt.Details
                where detail.StockMovementId != null
                select detail.StockMovementId;

            var stockMovements = _unitOfWork.StockMovements.Find(s => stockMovementIdsToRetrive.Contains(s.Id));
            await _unitOfWork.StockMovements.RemoveRange(stockMovements);

            await ChangeStatus("Pendent Recepcionar", receipt);

            return new GenericResponse(true, stockMovements);
        }
    }
}