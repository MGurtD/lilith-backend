using Api.Services;
using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Services
{
    internal struct DeliveryNoteEntities
    {
        internal Customer Customer;
        internal Exercise Exercise;
        internal Site Site;
        internal Lifecycle Lifecycle;
    }

    public class DeliveryNoteService : IDeliveryNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockMovementService _stockMovementService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly string lifecycleName = "DeliveryNote";

        public DeliveryNoteService(IUnitOfWork unitOfWork, IStockMovementService stockMovementService, ISalesOrderService salesOrderService)
        {
            _unitOfWork = unitOfWork;
            _stockMovementService = stockMovementService;
            _salesOrderService = salesOrderService;
        }

        public IEnumerable<DeliveryNote> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate && p.StatusId == statusId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.CreatedOn >= startDate && p.CreatedOn <= endDate && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetByStatus(Guid statusId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.StatusId == statusId);
            return deliveryNotes;
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var deliveryNoteEntities = (DeliveryNoteEntities)response.Content!;

            var counter = deliveryNoteEntities.Exercise.DeliveryNoteCounter == 0 ? 1 : deliveryNoteEntities.Exercise.DeliveryNoteCounter;
            var deliveryNote = new DeliveryNote()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                CustomerId = createRequest.CustomerId,
                SiteId = deliveryNoteEntities.Site.Id,
                CreatedOn = createRequest.Date,
                Number = $"{deliveryNoteEntities.Exercise.Name}-{counter.ToString().PadLeft(3, '0')}",
                StatusId = deliveryNoteEntities.Lifecycle.InitialStatusId!.Value
            };
            await _unitOfWork.DeliveryNotes.Add(deliveryNote);

            // Incrementar 
            deliveryNoteEntities.Exercise.DeliveryNoteCounter = counter + 1;
            await _unitOfWork.Exercices.Update(deliveryNoteEntities.Exercise);

            return new GenericResponse(true, deliveryNote);
        }

        public async Task<GenericResponse> Update(DeliveryNote deliveryNote)
        {
            // Netejar dependencies per evitar col·lisions de EF
            deliveryNote.Details?.Clear();

            var exists = await _unitOfWork.DeliveryNotes.Exists(deliveryNote.Id);
            if (!exists) return new GenericResponse(false, $"Id {deliveryNote.Id} inexistent" );

            await _unitOfWork.DeliveryNotes.Update(deliveryNote);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var deliveryNotes = await _unitOfWork.DeliveryNotes.Get(id);
            if (deliveryNotes == null) return new GenericResponse(false,  $"Id {id} inexistent" );

            await _unitOfWork.DeliveryNotes.Remove(deliveryNotes);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> MoveToWarehose(DeliveryNote deliveryNote)
        {
            var defaultLocation = _unitOfWork.Warehouses.Locations.Find(l => l.Default == true).FirstOrDefault();
            if (defaultLocation == null) return new GenericResponse(false, "No hi ha una ubicació per defecte");

            //var detailsToMove = deliveryNote.Details!.Where(d => d.StockMovementId == null);
            //foreach (var detail in detailsToMove)
            //{
            //    var stockMovement = new StockMovement
            //    {
            //        LocationId = defaultLocation.Id,
            //        MovementDate = DateTime.Now,
            //        CreatedOn = DateTime.Now,
            //        MovementType = StockMovementType.INPUT,
            //        Description = $"Albará {deliveryNote.Number}"
            //    };
            //    stockMovement.SetFromReceiptDetail(detail);
            //    detail.Reference = null;
            //    detail.StockMovementId = stockMovement.Id;

            //    await _stockMovementService.Create(stockMovement);
            //    await _unitOfWork.Receipts.Details.Update(detail);
            //}

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> RetriveFromWarehose(DeliveryNote deliveryNote)
        {
            //var detailsToRetrive = deliveryNote.Details!.Where(d => d.StockMovementId != null);
            //foreach (var detail in detailsToRetrive)
            //{
            //    Guid oldStockMovementId = detail.StockMovementId!.Value;

            //    detail.Reference = null;
            //    detail.StockMovementId = null;
            //    await _unitOfWork.Receipts.Details.Update(detail);
            //    await _stockMovementService.Remove(oldStockMovementId);
            //}

            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateHeaderRequest createInvoiceRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createInvoiceRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, new List<string>() { "L'exercici no existex" });

            var customer = await _unitOfWork.Customers.Get(createInvoiceRequest.CustomerId);
            if (customer == null) return new GenericResponse(false, new List<string>() { "El client no existeix" });
            if (!customer.IsValidForSales())
                return new GenericResponse(false, new List<string>() { "El client no és válid per a crear una factura. Revisa el nom fiscal, el número de compte i el NIF" });
            if (customer.MainAddress() == null)
                return new GenericResponse(false, new List<string>() { "El client no té direccions donades d'alta. Si us plau, creí una direcció." });

            var site = _unitOfWork.Sites.Find(s => s.Name == "Local Torelló").FirstOrDefault();
            if (site == null)
                return new GenericResponse(false, new List<string>() { "La seu 'Temges' no existeix" });
            if (!site.IsValidForSales())
                return new GenericResponse(false, new List<string>() { "Seu 'Temges' no és válida per crear una factura. Revisi les dades de facturació." });

            var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == lifecycleName).FirstOrDefault();
            if (lifecycle == null) return new GenericResponse(false, $"Cicle de vida '{lifecycleName}' inexistent");
            if (!lifecycle.InitialStatusId.HasValue) return new GenericResponse(false, $"El cicle de vida '{lifecycleName}' no té un estat inicial");

            DeliveryNoteEntities entities;
            entities.Exercise = exercise;
            entities.Customer = customer;
            entities.Site = site;
            entities.Lifecycle = lifecycle;
            return new GenericResponse(true, entities);
        }

        public async Task<GenericResponse> AddOrder(Guid deliveryNoteId, SalesOrderHeader order)
        {
            // Crear lines de l'albarà segons les línies de la comanda
            var deliveryNoteDetails = new List<DeliveryNoteDetail>();
            foreach (var salesDetail in order.SalesOrderDetails)
            {
                var deliveryNoteDetail = new DeliveryNoteDetail
                {
                    DeliveryNoteId = deliveryNoteId
                };
                deliveryNoteDetail.SetFromOrderDetail(salesDetail);
                deliveryNoteDetails.Add(deliveryNoteDetail);
            }
            await _unitOfWork.DeliveryNotes.Details.AddRange(deliveryNoteDetails);

            // Associar albarà a comanda per evitar la selecció d'altres comandes
            order.DeliveryNoteId = deliveryNoteId;
            await _salesOrderService.Update(order);

            return new GenericResponse(true, deliveryNoteDetails);
        }

        public async Task<GenericResponse> RemoveOrder(Guid deliveryNoteId, SalesOrderHeader order)
        {
            var detailIds = order.SalesOrderDetails.Select(d => d.Id).ToList();

            var deliveryNoteDetails = _unitOfWork.DeliveryNotes.Details.Find(d => detailIds.Contains(d.SalesOrderDetailId));
            // Eliminar en bloc
            await _unitOfWork.DeliveryNotes.Details.RemoveRange(deliveryNoteDetails);

            // Alliberar la comanda perquè sigui assignable de nou a un albarà
            order.DeliveryNoteId = null;
            await _salesOrderService.Update(order);

            return new GenericResponse(true, deliveryNoteDetails);
        }
    }
}