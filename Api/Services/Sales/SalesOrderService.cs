using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Sales
{
    public class SalesOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService) : ISalesOrderService
    {
        public async Task<SalesOrderReportResponse?> GetByIdForReporting(Guid id)
        {
            var salesOrder = await GetById(id);
            if (salesOrder is null) return null;

            if (!salesOrder.CustomerId.HasValue) return null;
            var customer = await unitOfWork.Customers.Get(salesOrder.CustomerId.Value);
            if (customer is null) return null;

            if (!salesOrder.SiteId.HasValue) return null;
            var site = await unitOfWork.Sites.Get(salesOrder.SiteId.Value);
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
            var salesOrderHeader = await unitOfWork.SalesOrderHeaders.Get(id);
            return salesOrderHeader;
        }

        public SalesOrderHeader? GetOrderFromBudget(Guid budgetId)
        {
            var salesOrder = unitOfWork.SalesOrderHeaders.Find(p => p.BudgetId == budgetId).FirstOrDefault();
            return salesOrder;
        }

        public IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var salesOrderHeaders = unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate);
            return salesOrderHeaders;
        }
        public IEnumerable<SalesOrderHeader> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var invoices = unitOfWork.SalesOrderHeaders.Find(p => p.Date >= startDate && p.Date <= endDate && p.CustomerId == customerId);
            return invoices;
        }

        public IEnumerable<SalesOrderHeader> GetByDeliveryNoteId(Guid deliveryNoteId)
        {
            var orders = unitOfWork.SalesOrderHeaders.Find(p => p.DeliveryNoteId == deliveryNoteId);
            return orders;
        }

        public IEnumerable<SalesOrderHeader> GetOrdersToDeliver(Guid customerId)
        {
            var orders = unitOfWork.SalesOrderHeaders.Find(p => p.CustomerId == customerId && p.DeliveryNoteId == null);
            return orders;
        }

        public async Task<GenericResponse> CreateFromBudget(Budget budget)
        {
            var createDto = new CreateHeaderRequest
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                CustomerId = budget.CustomerId
            };

            // Obtenir l'exercici actual pel nou document
            var currentExercise = exerciseService.GetExerciceByDate(createDto.Date);
            if (currentExercise == null)
            {
                return new GenericResponse(false, "No s'ha trobat cap exercici per a la data actual");
            }
            createDto.ExerciseId = currentExercise.Id;   

            var createResponse = await Create(createDto);
            if (!createResponse.Result) return createResponse;

            var salesOrder = (SalesOrderHeader)createResponse.Content!;
            salesOrder.ExpectedDate = DateTime.Now.AddDays(budget.DeliveryDays);
            salesOrder.BudgetId = budget.Id;
            var updateResponse = await Update(salesOrder);
            if (!updateResponse.Result) return updateResponse;

            foreach (var detail in budget.Details)
            {
                var salesOrderDetail = new SalesOrderDetail(detail, DateTime.Now.AddDays(budget.DeliveryDays))
                {
                    SalesOrderHeaderId = salesOrder.Id
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

            var counterObj = await exerciseService.GetNextCounter(orderEntities.Exercise.Id, "salesorder");
            if (!counterObj.Result || counterObj.Content == null) return new GenericResponse(false, "Error al crear el comptador");
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
                var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == "SalesOrder").FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, "El cicle de vida 'SalesOrder' no existeix");
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, "El cicle de vida 'SalesOrder' no té estat inicial" );
                order.StatusId = lifecycle.InitialStatusId;
            }

            order.ExerciseId = orderEntities.Exercise.Id;
            order.SetCustomer(orderEntities.Customer);
            order.SetSite(orderEntities.Site);

            await unitOfWork.SalesOrderHeaders.Add(order);

            return new GenericResponse(true, order);
        }

        public async Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader)
        {
            salesOrderHeader.SalesOrderDetails.Clear();

            await unitOfWork.SalesOrderHeaders.Update(salesOrderHeader);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var salesOrder = await unitOfWork.SalesOrderHeaders.Get(id);
            if (salesOrder == null)
            {
                return new GenericResponse(false, $"La comanda amb ID {id} no existeix");
            }
            else
            {                
                await unitOfWork.SalesOrderHeaders.Remove(salesOrder);
                return new GenericResponse(true, new List<string> { });
            }
        }

        public async Task<GenericResponse> UpdateCosts(Guid id)
        {
            var details = unitOfWork.SalesOrderDetails.Find(e => e.SalesOrderHeaderId == id).ToList();

            foreach(SalesOrderDetail detail in details)
            {
                if (detail.WorkOrderId.HasValue)
                {
                    var workOrder = await unitOfWork.WorkOrders.Get(detail.WorkOrderId.Value);
                    if(workOrder != null)
                    {
                        detail.LastCost = (workOrder.MaterialCost + workOrder.OperatorCost + workOrder.MachineCost);
                    }
                    var workMaster = unitOfWork.WorkMasters.Find(e => e.ReferenceId == detail.ReferenceId).FirstOrDefault();
                    if (workMaster != null)
                    {
                        detail.WorkMasterCost = (workMaster.materialCost + workMaster.machineCost + workMaster.operatorCost + workMaster.externalCost);
                    }                                        
                    await unitOfWork.SalesOrderHeaders.UpdateDetail(detail);
                }
                
            }

            return new GenericResponse(true);
        }

        public async Task<SalesOrderDetail?> GetDetailById(Guid id)
        {
            var detail = await unitOfWork.SalesOrderDetails.Get(id);
            return detail;
        }
        public async Task<GenericResponse> AddDetail(SalesOrderDetail salesOrderDetail)
        {
            await unitOfWork.SalesOrderHeaders.AddDetail(salesOrderDetail);

            return new GenericResponse(true);
        }
        public async Task<GenericResponse> UpdateDetail(SalesOrderDetail salesOrderDetail)
        {
            salesOrderDetail.Reference = null;
            salesOrderDetail.SalesOrderHeader = null;

            await unitOfWork.SalesOrderHeaders.UpdateDetail(salesOrderDetail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = unitOfWork.SalesOrderDetails.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, new List<string> { $"El detall de comanda amb ID {id} no existeix" });
            var deleted = await unitOfWork.SalesOrderHeaders.RemoveDetail(detail);

            return new GenericResponse(true, detail);
        }

        private async Task<GenericResponse> GetStatusId(string statusName)
        {
            var lifecycle = await unitOfWork.Lifecycles.GetByName("SalesOrder");
            if (lifecycle == null) return new GenericResponse(false, "El cicle de vida 'SalesOrder' no existeix");

            var status = lifecycle.Statuses!.FirstOrDefault(s => s.Name == statusName);
            if (status == null) return new GenericResponse(false, $"L'estat {statusName} no existeix dins del cicle de vida 'SalesOrder'");

            return new GenericResponse(true, status.Id);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateHeaderRequest createInvoiceRequest)
        {
            if (createInvoiceRequest.Date == DateTime.MinValue) return new GenericResponse(false, "La data de la comanda no pot ser buida");

            var exercise = await unitOfWork.Exercices.Get(createInvoiceRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, "L'exercici no existex" );

            var customer = await unitOfWork.Customers.Get(createInvoiceRequest.CustomerId);
            if (customer == null) return new GenericResponse(false, "El client no existeix" );
            if (!customer.IsValidForSales())
                return new GenericResponse(false, "El client no és válid per a crear una factura. Revisa el nom fiscal, el número de compte i el NIF" );
            if (customer.MainAddress() == null)
                return new GenericResponse(false,  "El client no té direccions donades d'alta. Si us plau, creí una direcció." );

            var site = unitOfWork.Sites.Find(s => s.Name == "Local Torelló").FirstOrDefault();
            if (site == null)
                return new GenericResponse(false, "La seu 'Temges' no existeix" );
            if (!site.IsValidForSales())
                return new GenericResponse(false, "Seu 'Temges' no és válida per crear una factura. Revisi les dades de facturació." );

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
                // Actualizar flag de 'servida' en els detalls
                foreach (var detail in order.SalesOrderDetails)
                {
                    detail.IsDelivered = isDelivered;
                    await UpdateDetail(detail);
                }

                // Canviar estat de la comanda
                order.StatusId = (Guid)statusResponse.Content!;
                // Asociar albarà
                if (order.DeliveryNoteId == null && isDelivered) order.DeliveryNoteId = deliveryNoteId;
                await Update(order);
            }

            return new GenericResponse(true);
        }

        #endregion

    }
}
