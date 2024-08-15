using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services;
using Application.Services.Purchase;
using Domain.Entities.Purchase;

namespace Api.Services.Purchase
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExerciseService _exerciseService;
        private readonly string LifecycleName = "PurchaseOrder";

        public PurchaseOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService)
        {
            _unitOfWork = unitOfWork;
            _exerciseService = exerciseService;
        }

        public async Task<List<PurchaseOrder>> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? supplierId, Guid? statusId)
        {
            var orders = new List<PurchaseOrder>();
            if (supplierId.HasValue)
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate && p.SupplierId == supplierId);
            }
            else if (statusId.HasValue)
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate && p.StatusId == statusId);
            }
            else
            {
                orders = await _unitOfWork.PurchaseOrders.FindAsync(p => p.Date >= startDate && p.Date <= endDate);
            }
            return orders;
        }

        public async Task<List<PurchaseOrder>> GetBySupplier(Guid supplierId)
        {
            return await _unitOfWork.PurchaseOrders.FindAsync(p => p.SupplierId == supplierId);
        }

        public async Task<GenericResponse> Create(CreatePurchaseDocumentRequest createRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, "Exercici inexistent" );

            var status = _unitOfWork.Lifecycles.Find(l => l.Name == LifecycleName).FirstOrDefault();
            if (status == null) return new GenericResponse(false, $"Cicle de vida '{LifecycleName}' inexistent" );
            if (!status.InitialStatusId.HasValue) return new GenericResponse(false, $"El cicle de vida '{LifecycleName}' no té un estat inicial" );

            var counterObj = await _exerciseService.GetNextCounter(exercise.Id, "purchaseorder");
            if (counterObj == null || counterObj.Content == null) return new GenericResponse(false, "Error al crear el comptador");

            var orderNumber = counterObj.Content.ToString()!;
            var order = new PurchaseOrder()
            {
                Id = createRequest.Id,
                ExerciseId = createRequest.ExerciseId,
                SupplierId = createRequest.SupplierId,
                Date = createRequest.Date,
                Number = orderNumber,
                StatusId = status.InitialStatusId.Value
            };
            await _unitOfWork.PurchaseOrders.Add(order);

            return new GenericResponse(true);
        }
        public async Task<GenericResponse> Update(PurchaseOrder order)
        {
            // Netejar dependencies per evitar col·lisions de EF
            order.Details?.Clear();

            var exists = await _unitOfWork.PurchaseOrders.Exists(order.Id);
            if (!exists) return new GenericResponse(false, $"Id {order.Id} inexistent" );

            await _unitOfWork.PurchaseOrders.Update(order);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> Remove(Guid id)
        {
            var receipt = await _unitOfWork.PurchaseOrders.Get(id);
            if (receipt == null) return new GenericResponse(false, $"Id {id} inexistent" );

            await _unitOfWork.PurchaseOrders.Remove(receipt);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> AddDetail(PurchaseOrderDetail detail)
        {
            detail.Reference = null;

            await _unitOfWork.PurchaseOrders.Details.Add(detail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> UpdateDetail(PurchaseOrderDetail detail)
        {
            var exists = await _unitOfWork.PurchaseOrders.Details.Exists(detail.Id);
            if (!exists) return new GenericResponse(false, $"Id {detail.Id} inexistent");

            detail.Reference = null;
            await _unitOfWork.PurchaseOrders.Details.Update(detail);
            return new GenericResponse(true);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = await _unitOfWork.PurchaseOrders.Details.Get(id);
            if (detail == null) return new GenericResponse(false, $"Id {id} inexistent");

            await _unitOfWork.PurchaseOrders.Details.Remove(detail);
            return new GenericResponse(true);
        }
        
    }
}