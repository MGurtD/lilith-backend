using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Warehouse;

namespace Api.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockService _stockService;
        private readonly Location? _defaultLocation;

        public StockMovementService(IUnitOfWork unitOfWork, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _stockService = stockService;

            _defaultLocation = _unitOfWork.Warehouses.Locations.Find(l => l.Default).FirstOrDefault();
        }

        public async Task<GenericResponse> Create(StockMovement request)
        {
            if (_defaultLocation == null) return new GenericResponse(false, "No hi ha una ubicaci� per defecte definida al projecte");
            request.LocationId = _defaultLocation.Id;

            // Comprovar si existeix un stock id per les dimensions i producte
            var stock = _stockService.GetByDimensions(_defaultLocation.Id, request.ReferenceId, 
                                                      request.Width, request.Length, request.Height,
                                                      request.Diameter, request.Thickness);

            if (request.MovementType == StockMovementType.OUTPUT)
            {
                if (request.Quantity > 0) request.Quantity *= -1;
            }
            else if (request.MovementType == StockMovementType.INPUT)
            {
                if (request.Quantity < 0) request.Quantity *= -1;
            }

            if (stock != null)
            {
                stock.Quantity += request.Quantity;
                await _stockService.Update(stock);

                request.StockId = stock.Id;
            }
            else
            {            
                var newStock = new Stock
                {
                    ReferenceId = request.ReferenceId,
                    LocationId = _defaultLocation.Id,
                    Quantity = request.Quantity,
                    Width = request.Width,
                    Length = request.Length,
                    Height = request.Height,
                    Diameter = request.Diameter,
                    Thickness = request.Thickness
                };                
                await _stockService.Create(newStock);

                request.StockId = newStock.Id;
            }

            
            await _unitOfWork.StockMovements.Add(request);
            return new GenericResponse(true, request);
        }

         public IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var stockMovements = _unitOfWork.StockMovements.Find(p => p.MovementDate >= startDate && p.MovementDate <= endDate);
            return stockMovements;
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            if (_defaultLocation == null) return new GenericResponse(false, "No hi ha una ubicaci� per defecte definida al projecte");

            var stockMovement = await _unitOfWork.StockMovements.Get(id);
            if ( stockMovement == null) return new GenericResponse(false, new List<string>() { $"Id {id} inexistent" });
            var stock = _stockService.GetByDimensions(_defaultLocation.Id, stockMovement.ReferenceId, 
                                                      stockMovement.Width, stockMovement.Length, stockMovement.Height,
                                                      stockMovement.Diameter, stockMovement.Thickness);

            stock.Quantity += (-1) * stockMovement.Quantity;
            await _stockService.Update(stock);

            await _unitOfWork.StockMovements.Remove(stockMovement);
            return new GenericResponse(true);
        }
    }
}