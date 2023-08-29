using Application.Contracts;
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
        public IEnumerable<SalesOrderHeader> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.SalesOrderHeaders.Find(p => p.SalesOrderDate >= startDate && p.SalesOrderDate <= endDate && p.StatusId == statusId);
            return invoices;
        }

        public async Task<GenericResponse> Create(SalesOrderHeader salesOrderHeader)
        {
            if (salesOrderHeader == null )
            {
                return new GenericResponse(false, new List<string> { $"Comanda invalida" });
            }
            if (!salesOrderHeader.ExerciseId.HasValue)
            {
                return new GenericResponse(false, new List<string> { $"Exercici invàlid" });

            }

            var exerciseId = salesOrderHeader.ExerciseId.Value;
            var exercise = _unitOfWork.Exercices.Find(e => e.Id == exerciseId).FirstOrDefault();
            salesOrderHeader.SalesOrderNumber = exercise.SalesOrderCounter;
            await _unitOfWork.SalesOrderHeaders.Add(salesOrderHeader);

            // Incrementar el comptador de comandes de l'exercici
            exercise.SalesOrderCounter += 1;
            await _unitOfWork.Exercices.Update(exercise);

            return new GenericResponse(true, new List<string> { });
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
            var orderDetail = await _unitOfWork.SalesOrderDetails.Get(id);
            if (orderDetail == null) return new GenericResponse(false, new List<string>() { $"La línia de comanda amb ID '{id}' no existeix" });
            await _unitOfWork.SalesOrderHeaders.RemoveDetail(orderDetail.Id);

            await RecalculateOrderStatus(orderDetail.SalesOrderHeaderId);
            return new GenericResponse(true, new List<string> { });
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

                if (initialStatusId != order.StatusId)
                    await _unitOfWork.SalesOrderHeaders.Update(order);
            }            

            return new GenericResponse(true, order);
        }
    }
}
