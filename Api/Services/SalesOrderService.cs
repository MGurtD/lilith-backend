using Application.Contracts.Auth;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;

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
        public async Task<GenericResponse>AddDetail(SalesOrderDetail salesOrderDetail)
        {
            await _unitOfWork.SalesOrderHeaders.AddDetail(salesOrderDetail);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> UpdateDetail(SalesOrderDetail salesOrderDetail)
        {
            await _unitOfWork.SalesOrderHeaders.UpdateDetail(salesOrderDetail);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            await _unitOfWork.SalesOrderHeaders.RemoveDetail(id);
            return new GenericResponse(true, new List<string> { });
        }

    }
}
