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
        private readonly IPurchaseOrderService _orderService;

        public ReceiptService(IUnitOfWork unitOfWork, IStockMovementService stockMovementService, IExerciseService exerciseService, IPurchaseOrderService orderService)
        {
            _unitOfWork = unitOfWork;
            _stockMovementService = stockMovementService;
            _exerciseService = exerciseService;
            _orderService = orderService;
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

        public async Task<GenericResponse> Create(CreatePurchaseDocumentRequest createRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, new List<string>() { "Exercici inexistent" });

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == "Receipts").FirstOrDefault();
            if (status == null) return new GenericResponse(false, new List<string>() { "Cicle de vida 'Receipts' inexistent" });
            if (!status.InitialStatusId.HasValue) return new GenericResponse(false, new List<string>() { "El cicle de vida 'Receipts' no té un estat inicial" });

            //var receiptCounter = exercise.ReceiptCounter == 0 ? 1 : exercise.ReceiptCounter;
            var counterObj = await _exerciseService.GetNextCounter(exercise.Id, "receipt");
            if (counterObj == null || counterObj.Content == null) return new GenericResponse(false, "Error al crear el comptador");

            var receiptCounter = counterObj.Content.ToString();
            var receipt = new Receipt()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                SupplierId = createRequest.SupplierId,
                Date = createRequest.Date,
                Number = receiptCounter!,
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

            try {
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
            } catch (Exception e) {
                return new GenericResponse(false, e.Message);
            }
        }

        public async Task<GenericResponse> AddDetail(ReceiptDetail detail)
        {
            detail.Reference = null;

            await _unitOfWork.Receipts.Details.Add(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UpdateDetail(ReceiptDetail detail)
        {
            var exists = await _unitOfWork.Receipts.Details.Exists(detail.Id);
            if (!exists) return new GenericResponse(false, $"Id {detail.Id} inexistent");

            detail.Reference = null;
            await _unitOfWork.Receipts.Details.Update(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            // Obtenir detall
            var detail = await _unitOfWork.Receipts.Details.Get(id);
            if (detail == null) return new GenericResponse(false, $"Id {id} inexistent");

            // Comprobar si el detall té recepcions associades
            var reception = (await _unitOfWork.PurchaseOrders.Receptions.FindAsync(r => r.ReceiptDetailId == id)).FirstOrDefault();
            if (reception != null)
            {
                var response = await _orderService.RemoveReception(reception);
                if (!response.Result) return response;
            }            

            await _unitOfWork.Receipts.Details.Remove(detail);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> MoveToWarehose(Receipt receipt)
        {
            var defaultLocation = await _unitOfWork.Warehouses.GetDefaultLocation();
            if (defaultLocation == null) return new GenericResponse(false, "No hi ha una ubicació per defecte");

            var detailsToMove = receipt.Details!.Where(d => d.StockMovementId == null);
            foreach (var detail in detailsToMove)
            {
                var stockMovement = new StockMovement
                {
                    LocationId = defaultLocation.Id,
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

        public async Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id)
        {
            return await _unitOfWork.Receipts.GetReceptions(id);
        }

        public async Task<GenericResponse> AddReceptions(AddReceptionsRequest request)         
        {
            // Obtenir els detalls de la comanda
            var orderDetails = await _unitOfWork.PurchaseOrders.GetOrderDetailsFromReceptions(request.Receptions.Select(r => r.PurchaseOrderDetailId).ToList());

            foreach (var reception in request.Receptions)
            {
                var detail = orderDetails.FirstOrDefault(orderDetails => orderDetails.Id == reception.PurchaseOrderDetailId);
                if (detail == null) return new GenericResponse(false, $"Detall {reception.PurchaseOrderDetailId} inexistent");

                // Actualizar detall de la comanda
                var orderDetailResponse = await _orderService.AddReceivedQuantityAndCalculateStatus(detail, (int)reception.Quantity);
                if (!orderDetailResponse.Result) return orderDetailResponse;

                // Crear detall al albarà
                var receiptDetail = new ReceiptDetail
                {
                    Id = reception.ReceiptDetailId,
                    ReceiptId = request.ReceiptId,
                    ReferenceId = detail.ReferenceId,
                    Description = detail.Description,
                    Quantity = (int) reception.Quantity,
                    UnitPrice = reception.UnitPrice > 0 ? reception.UnitPrice : detail.UnitPrice,
                    Amount = reception.Price,
                };              
                await _unitOfWork.Receipts.Details.Add(receiptDetail);

                // Afegir recepció a la comanda
                await _unitOfWork.PurchaseOrders.Receptions.Add(new PurchaseOrderReceiptDetail
                {
                    PurchaseOrderDetailId = detail.Id,
                    ReceiptDetailId = receiptDetail.Id,
                    Quantity = (int)reception.Quantity,
                    User = reception.User,
                    CreatedOn = DateTime.Now
                });
            }

            // Crear moviments de magatzem
            var receipt = await _unitOfWork.Receipts.Get(request.ReceiptId);
            if (receipt == null) return new GenericResponse(false, $"Albará {request.ReceiptId} inexistent");

            // Recalcular l'estat de la comanda
            return await _orderService.DeterminateStatus(orderDetails.First().PurchaseOrderId);
        }

    }
}