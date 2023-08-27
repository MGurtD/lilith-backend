using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Sales;
using SalesOrderDetail = Domain.Entities.Sales.SalesOrderDetail;

namespace Api.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SalesOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<SalesOrderHeader?> GetById(Guid id)
        {
            var salesOrderHeader = await _unitOfWork.SalesOrderHeaders.Get(id);
            return salesOrderHeader;
        }

        public IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var salesOrderHeaders = _unitOfWork.SalesOrderHeaders.Find(p => p.SalesOrderDate >= startDate && p.SalesOrderDate <= endDate);
            return salesOrderHeaders;
        }
        public IEnumerable<SalesOrderHeader> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.SalesOrderHeaders.Find(p => p.SalesOrderDate >= startDate && p.SalesOrderDate <= endDate && p.StatusId == statusId);
            return invoices;
        }

        public async Task<GenericResponse> Create(CreateOrderOrInvoiceRequest createRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createRequest);
            if (!response.Result) return response;

            var invoiceEntities = (InvoiceEntities)response.Content!;
            var order = new SalesOrderHeader
            {
                Id = createRequest.Id,
                SalesOrderNumber = invoiceEntities.Exercise.SalesOrderCounter + 1,
                SalesOrderDate = createRequest.Date
            };

            // Estat inicial
            if (createRequest.InitialStatusId.HasValue)
            {
                order.StatusId = createRequest.InitialStatusId;
            } else
            {
                var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == "SalesOrder").FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesOrder' no existeix" });
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesOrder' no té estat inicial" });
                order.StatusId = lifecycle.InitialStatusId;
            }            

            order.ExerciseId = invoiceEntities.Exercise.Id;
            order.SetCustomer(invoiceEntities.Customer);
            order.SetSite(invoiceEntities.Site);

            await _unitOfWork.SalesOrderHeaders.Add(order);

            // Incrementar el comptador de comandes de l'exercici
            invoiceEntities.Exercise.SalesOrderCounter += 1;
            await _unitOfWork.Exercices.Update(invoiceEntities.Exercise);

            return new GenericResponse(true, order);
        }

        public async Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader)
        {            
            salesOrderHeader.SalesOrderDetails = null;

            await _unitOfWork.SalesOrderHeaders.Update(salesOrderHeader);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var salesOrder = _unitOfWork.SalesOrderHeaders.Find(p => p.Id == id).FirstOrDefault();
            if (salesOrder == null)
            {
                return new GenericResponse(false, new List<string> { $"La comanda amb ID {id} no existeix" });
            }
            else
            {
                await _unitOfWork.SalesOrderHeaders.Remove(salesOrder);
                return new GenericResponse(true, new List<string> { });
            }

        }

        public async Task<SalesOrderDetail?> GetDetailById(Guid id)
        {
            var detail = await _unitOfWork.SalesOrderDetails.Get(id);
            return detail;
        }
        public async Task<GenericResponse>AddDetail(SalesOrderDetail salesOrderDetail)
        {
            await _unitOfWork.SalesOrderHeaders.AddDetail(salesOrderDetail);
            await RecalculateOrderStatus(salesOrderDetail.SalesOrderHeaderId);

            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> UpdateDetail(SalesOrderDetail salesOrderDetail)
        {
            await _unitOfWork.SalesOrderHeaders.UpdateDetail(salesOrderDetail);
            await RecalculateOrderStatus(salesOrderDetail.SalesOrderHeaderId);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = _unitOfWork.SalesOrderDetails.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, new List<string> { $"El detall de comanda amb ID {id} no existeix" });
            var deleted = await _unitOfWork.SalesOrderHeaders.RemoveDetail(detail);

            await RecalculateOrderStatus(detail!.SalesOrderHeaderId);
            return new GenericResponse(true, detail);
        }

        private async Task<GenericResponse> RecalculateOrderStatus(Guid orderId)
        {
            var order = _unitOfWork.SalesOrderHeaders.Find(o => o.Id == orderId).FirstOrDefault();
            if (order == null) return new GenericResponse(false, new List<string>() { $"La comanda amb ID '{orderId}' no existeix" });

            var initialStatusId = order.StatusId;
            var numberOfDetails = order.SalesOrderDetails.Count();
            if (numberOfDetails > 0)
            {
                var numberOfInvoicedDetails = order.SalesOrderDetails.Where(d => d.IsInvoiced).Count();
                var numberOfServedDetails = order.SalesOrderDetails.Where(d => d.IsServed).Count();

                var lifecycle = await _unitOfWork.Lifecycles.GetByName("SalesOrder");
                if (lifecycle == null) return new GenericResponse(false, new List<string>());

                var currentStatus = lifecycle.Statuses!.First(s => s.Id == order.StatusId!.Value);
                if (numberOfDetails == numberOfInvoicedDetails)
                {
                    order.StatusId = lifecycle.Statuses!.First(s => s.Name == "Comanda Facturada").Id;
                }
                else if (numberOfInvoicedDetails > 0)
                {
                    order.StatusId = lifecycle.Statuses!.First(s => s.Name == "Comanda Parcialment Facturada").Id;
                }
                else if (numberOfDetails == numberOfServedDetails)
                {
                    order.StatusId = lifecycle.Statuses!.First(s => s.Name == "Comanda Servida").Id;
                }
                else if (numberOfServedDetails > 0)
                {
                    order.StatusId = lifecycle.Statuses!.First(s => s.Name == "Comanda Parcialment Servida").Id;
                }

                order.SalesOrderDetails.Clear();
                if (initialStatusId != order.StatusId)
                    await _unitOfWork.SalesOrderHeaders.Update(order);
            }            

            return new GenericResponse(true, order);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateOrderOrInvoiceRequest createInvoiceRequest)
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
    }
}
