﻿using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services.Sales;
using Application.Services.Warehouse;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Domain.Entities.Warehouse;

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
        private readonly IExerciseService _exerciseService;
        private readonly string lifecycleName = "DeliveryNote";

        public DeliveryNoteService(IUnitOfWork unitOfWork, IStockMovementService stockMovementService, ISalesOrderService salesOrderService, IExerciseService exerciseService)
        {
            _unitOfWork = unitOfWork;
            _stockMovementService = stockMovementService;
            _salesOrderService = salesOrderService;
            _exerciseService = exerciseService;
        }

        public async Task<DeliveryNoteReportResponse?> GetDtoForReportingById(Guid id)
        {
            var deliveryNote = await _unitOfWork.DeliveryNotes.Get(id);
            if (deliveryNote is null) return null;

            var orders = _salesOrderService.GetByDeliveryNoteId(id);

            var customer = await _unitOfWork.Customers.Get(deliveryNote.CustomerId);
            if (customer is null) return null;

            var site = await _unitOfWork.Sites.Get(deliveryNote.SiteId);
            if (site is null) return null;

            var deliveryNoteOrders = orders.Select(order => new DeliveryNoteOrderReportDto
            {
                Number = order.Number,
                Date = order.Date,
                CustomerNumber = order.CustomerNumber,
                Details = order.SalesOrderDetails.ToList(),
                Total = order.SalesOrderDetails.Sum(d => d.Amount)
            }).ToList();

            var deliveryNoteReport = new DeliveryNoteReportResponse
            {
                DeliveryNote = deliveryNote,
                Orders = deliveryNoteOrders,
                Customer = customer,
                Site = site,
                Total = deliveryNote.Details.Sum(d => d.Amount),
            };
            return deliveryNoteReport;
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

        public IEnumerable<DeliveryNote> GetByCustomer(Guid customerId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetByStatusAndCustomer(Guid statusId, Guid customerId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.StatusId == statusId && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetBySalesInvoice(Guid salesInvoiceId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.GetByInvoiceId(salesInvoiceId);
            return deliveryNotes;
        }

        public IEnumerable<DeliveryNote> GetDeliveryNotesToInvoice(Guid customerId)
        {
            var deliveryNotes = _unitOfWork.DeliveryNotes.Find(p => p.SalesInvoiceId == null && p.CustomerId == customerId);
            return deliveryNotes;
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var deliveryNoteEntities = (DeliveryNoteEntities)response.Content!;

            var counterObj = await _exerciseService.GetNextCounter(deliveryNoteEntities.Exercise.Id, "deliverynote");
            if (counterObj.Content == null) return new GenericResponse(false, new List<string>() { "Error al crear el comptador" });
            var deliveryNote = new DeliveryNote()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                CustomerId = createRequest.CustomerId,
                SiteId = deliveryNoteEntities.Site.Id,
                CreatedOn = createRequest.Date,
                Number = counterObj.Content.ToString()!,
                StatusId = deliveryNoteEntities.Lifecycle.InitialStatusId!.Value
            };
            await _unitOfWork.DeliveryNotes.Add(deliveryNote);

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

        public async Task<GenericResponse> Deliver(DeliveryNote deliveryNote)
        {
            var warehouseResponse = await RetriveFromWarehose(deliveryNote);
            if (!warehouseResponse.Result) return warehouseResponse;

            var orderServiceResponse = await _salesOrderService.Deliver(deliveryNote.Id);
            if (!orderServiceResponse.Result) return orderServiceResponse;
            if (deliveryNote.DeliveryDate is null)
            {
                deliveryNote.DeliveryDate = DateTime.Now;
            }
            
            await Update(deliveryNote);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> UnDeliver(DeliveryNote deliveryNote)
        {
            var warehouseResponse = await ReturnToWarehose(deliveryNote);
            if (!warehouseResponse.Result) return warehouseResponse;

            var orderServiceResponse = await _salesOrderService.UnDeliver(deliveryNote.Id);
            if (!orderServiceResponse.Result) return orderServiceResponse;

            deliveryNote.DeliveryDate = null;
            await Update(deliveryNote);

            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Invoice(Guid salesInvoiceId)
        {
            return await ChangeInvoiceableStatus(salesInvoiceId, true);
        }

        public async Task<GenericResponse> UnInvoice(Guid salesInvoiceId)
        {
            return await ChangeInvoiceableStatus(salesInvoiceId, false);
        }

        public async Task<GenericResponse> ChangeInvoiceableStatus(Guid salesInvoiceId, bool isInvoiced)
        {
            var deliveryNotes = GetBySalesInvoice(salesInvoiceId);
            if (deliveryNotes != null && deliveryNotes.Any())
            {
                foreach (var deliveryNote in deliveryNotes.ToList())
                {
                    foreach (var detail in deliveryNote.Details)
                    {
                        detail.IsInvoiced = isInvoiced;
                        _unitOfWork.DeliveryNotes.Details.UpdateWithoutSave(detail);
                    }                    

                    await _unitOfWork.CompleteAsync();
                }
            }

            return new GenericResponse(true);
        }

        private async Task<GenericResponse> RetriveFromWarehose(DeliveryNote deliveryNote)
        {
            foreach (var detail in deliveryNote.Details)
            {
                detail.Reference = null;

                var stockMovement = new StockMovement
                {
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.OUTPUT,
                    Description = $"Albará {deliveryNote.Number}",
                    ReferenceId = detail.ReferenceId,
                    Quantity = detail.Quantity,
                };
                var response = await _stockMovementService.Create(stockMovement);
                if (!response.Result) return response;
            }

            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ReturnToWarehose(DeliveryNote deliveryNote)
        {
            foreach (var detail in deliveryNote.Details)
            {
                detail.Reference = null;

                var stockMovement = new StockMovement
                {
                    MovementDate = DateTime.Now,
                    CreatedOn = DateTime.Now,
                    MovementType = StockMovementType.INPUT,
                    Description = $"Retorn albará {deliveryNote.Number}",
                    ReferenceId = detail.ReferenceId,
                    Quantity = detail.Quantity,
                };
                var response = await _stockMovementService.Create(stockMovement);
                if (!response.Result) return response;
            }

            return new GenericResponse(true);
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

            var deliveryNoteDetails = _unitOfWork.DeliveryNotes.Details.Find(d => d.SalesOrderDetailId != null && detailIds.Contains(d.SalesOrderDetailId.Value));
            // Eliminar en bloc
            await _unitOfWork.DeliveryNotes.Details.RemoveRange(deliveryNoteDetails);

            // Alliberar la comanda perquè sigui assignable de nou a un albarà
            order.DeliveryNoteId = null;
            await _salesOrderService.Update(order);

            return new GenericResponse(true, deliveryNoteDetails);
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
    }
}