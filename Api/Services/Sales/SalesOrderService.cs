using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Sales;

namespace Api.Services.Sales
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExerciseService _exerciseService;

        public SalesOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService)
        {
            _unitOfWork = unitOfWork;
            _exerciseService = exerciseService;
        }        

        public async Task<SalesOrderReportResponse?> GetByIdForReporting(Guid id)
        {
            var salesOrder = await GetById(id);
            if (salesOrder is null) return null;

            if (!salesOrder.CustomerId.HasValue) return null;
            var customer = await _unitOfWork.Customers.Get(salesOrder.CustomerId.Value);
            if (customer is null) return null;

            if (!salesOrder.SiteId.HasValue) return null;
            var site = await _unitOfWork.Sites.Get(salesOrder.SiteId.Value);
            if (site is null) return null;

            salesOrder.SalesOrderDetails = salesOrder.SalesOrderDetails.OrderBy(d => d.Reference!.Code).ToList();
            var salesOrderReport = new SalesOrderReportResponse
            {
                Order = salesOrder,
                Customer = customer,
                Site = site,
                Total = salesOrder.SalesOrderDetails.Sum(d => d.Amount),
            };
            return salesOrderReport;
        }

        public async Task<SalesOrderHeader?> GetById(Guid id)
        {
            var salesOrderHeader = await _unitOfWork.SalesOrderHeaders.Get(id);
            return salesOrderHeader;
        }

        public SalesOrderHeader? GetOrderFromBudget(Guid budgetId)
        {
            var salesOrder = _unitOfWork.SalesOrderHeaders.Find(p => p.BudgetId == budgetId).FirstOrDefault();
            return salesOrder;
        }

        public IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var salesOrderHeaders = _unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate);
            return salesOrderHeaders;
        }
        public IEnumerable<SalesOrderHeader> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var invoices = _unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate && p.CustomerId == customerId);
            return invoices;
        }

        public IEnumerable<SalesOrderHeader> GetByDeliveryNoteId(Guid deliveryNoteId)
        {
            var orders = _unitOfWork.SalesOrderHeaders.Find(p => p.DeliveryNoteId == deliveryNoteId);
            return orders;
        }

        public IEnumerable<SalesOrderHeader> GetOrdersToDeliver(Guid customerId)
        {
            var orders = _unitOfWork.SalesOrderHeaders.Find(p => p.CustomerId == customerId && p.DeliveryNoteId == null);
            return orders;
        }

        public async Task<GenericResponse> CreateFromBudget(Budget budget)
        {
            var createDto = new CreateHeaderRequest
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                ExerciseId = budget.ExerciseId,
                CustomerId = budget.CustomerId
            };

            var createResponse = await Create(createDto);
            if (!createResponse.Result) return createResponse;

            var salesOrder = (SalesOrderHeader)createResponse.Content!;
            salesOrder.ExpectedDate = DateTime.Now.AddDays(budget.DeliveryDays);
            salesOrder.BudgetId = budget.Id;
            var updateResponse = await Update(salesOrder);
            if (!updateResponse.Result) return updateResponse;

            foreach (var detail in budget.Details)
            {
                var salesOrderDetail = new SalesOrderDetail
                {
                    Id = Guid.NewGuid(),
                    SalesOrderHeaderId = salesOrder.Id,
                    ReferenceId = detail.ReferenceId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Amount = detail.Amount,
                    Description = detail.Description,
                    IsDelivered = false,
                    UnitCost = detail.UnitCost,
                    TotalCost = detail.TotalCost,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };
                await AddDetail(salesOrderDetail);
            }

            return new GenericResponse(true, salesOrder);
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var orderEntities = (InvoiceEntities)response.Content!;

            var counterObj = await _exerciseService.GetNextCounter(orderEntities.Exercise.Id, "salesorder");
            if (!counterObj.Result || counterObj.Content == null) return new GenericResponse(false, new List<string>() { "Error al crear el comptador" });
            var order = new SalesOrderHeader
            {
                Id = createRequest.Id,
                Number = counterObj.Content.ToString()!,
                Date = createRequest.Date
            };

            // Estat inicial
            if (createRequest.InitialStatusId.HasValue)
            {
                order.StatusId = createRequest.InitialStatusId;
            }
            else
            {
                var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == "SalesOrder").FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesOrder' no existeix" });
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesOrder' no té estat inicial" });
                order.StatusId = lifecycle.InitialStatusId;
            }

            order.ExerciseId = orderEntities.Exercise.Id;
            order.SetCustomer(orderEntities.Customer);
            order.SetSite(orderEntities.Site);

            await _unitOfWork.SalesOrderHeaders.Add(order);

            return new GenericResponse(true, order);
        }

        public async Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader)
        {
            salesOrderHeader.SalesOrderDetails.Clear();

            await _unitOfWork.SalesOrderHeaders.Update(salesOrderHeader);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var salesOrder = _unitOfWork.SalesOrderHeaders.Find(p => p.Id == id).FirstOrDefault();
            if (salesOrder == null)
            {
                return new GenericResponse(false, $"La comanda amb ID {id} no existeix");
            }
            else
            {
                await _unitOfWork.SalesOrderHeaders.Remove(salesOrder);
                return new GenericResponse(true, new List<string> { });
            }
        }

        public async Task<GenericResponse> UpdateCosts(Guid id)
        {
            var details = _unitOfWork.SalesOrderDetails.Find(e => e.SalesOrderHeaderId == id).ToList();

            foreach(SalesOrderDetail detail in details)
            {
                if (detail.WorkOrderId.HasValue)
                {
                    var workOrder = await _unitOfWork.WorkOrders.Get(detail.WorkOrderId.Value);
                    if(workOrder != null)
                    {
                        detail.LastCost = (workOrder.MaterialCost + workOrder.OperatorCost + workOrder.MachineCost);
                    }
                    var workMaster = _unitOfWork.WorkMasters.Find(e => e.ReferenceId == detail.ReferenceId).FirstOrDefault();
                    if (workMaster != null)
                    {
                        detail.WorkMasterCost = (workMaster.materialCost + workMaster.machineCost + workMaster.operatorCost + workMaster.externalCost);
                    }                                        
                    await _unitOfWork.SalesOrderHeaders.UpdateDetail(detail);
                }
                
            }

            return new GenericResponse(true);
        }

        public async Task<SalesOrderDetail?> GetDetailById(Guid id)
        {
            var detail = await _unitOfWork.SalesOrderDetails.Get(id);
            return detail;
        }
        public async Task<GenericResponse> AddDetail(SalesOrderDetail salesOrderDetail)
        {
            await _unitOfWork.SalesOrderHeaders.AddDetail(salesOrderDetail);

            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> UpdateDetail(SalesOrderDetail salesOrderDetail)
        {
            salesOrderDetail.Reference = null;
            salesOrderDetail.SalesOrderHeader = null;

            await _unitOfWork.SalesOrderHeaders.UpdateDetail(salesOrderDetail);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = _unitOfWork.SalesOrderDetails.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, new List<string> { $"El detall de comanda amb ID {id} no existeix" });
            var deleted = await _unitOfWork.SalesOrderHeaders.RemoveDetail(detail);

            return new GenericResponse(true, detail);
        }

        private async Task<GenericResponse> GetStatusId(string statusName)
        {
            var lifecycle = await _unitOfWork.Lifecycles.GetByName("SalesOrder");
            if (lifecycle == null) return new GenericResponse(false, "El cicle de vida 'SalesOrder' no existeix");

            var status = lifecycle.Statuses!.FirstOrDefault(s => s.Name == statusName);
            if (status == null) return new GenericResponse(false, $"L'estat {statusName} no existeix dins del cicle de vida 'SalesOrder'");

            return new GenericResponse(true, status.Id);
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

            InvoiceEntities invoiceEntities;
            invoiceEntities.Exercise = exercise;
            invoiceEntities.Customer = customer;
            invoiceEntities.Site = site;
            return new GenericResponse(true, invoiceEntities);
        }

        #region DeliveryNote


        public async Task<GenericResponse> Deliver(Guid deliveryNoteId)
        {
            return await ChangeDeliveryStatus(deliveryNoteId, true);
        }

        public async Task<GenericResponse> UnDeliver(Guid deliveryNoteId)
        {
            return await ChangeDeliveryStatus(deliveryNoteId, false);
        }

        private async Task<GenericResponse> ChangeDeliveryStatus(Guid deliveryNoteId, bool isDelivered)
        {
            var orders = GetByDeliveryNoteId(deliveryNoteId);
            if (orders == null) return new GenericResponse(true, "No existeixen comandes amb l'albarà associat");

            var statusResponse = await GetStatusId(isDelivered ? "Comanda Servida" : "Comanda");
            if (!statusResponse.Result) return statusResponse;

            foreach (var order in orders.ToList())
            {
                foreach (var detail in order.SalesOrderDetails)
                {
                    detail.IsDelivered = isDelivered;
                    await UpdateDetail(detail);
                }
                order.DeliveryNoteId = isDelivered ? deliveryNoteId : null;
                order.StatusId = (Guid)statusResponse.Content!;
                await Update(order);
            }

            return new GenericResponse(true);
        }

        #endregion

    }
}
