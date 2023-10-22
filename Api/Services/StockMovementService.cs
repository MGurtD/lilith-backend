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

        public StockMovementService(IUnitOfWork unitOfWork, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _stockService = stockService;
        }

        public async Task<GenericResponse> Create(StockMovement request)
        {
            //Comprovar si existeix un stock id per les dimensions i producte
            //Si existeix update de la quantitat
            //Si no existeix add stock
            
            var stock = await _stockService.GetByDimensions(request.LocationId, request.ReferenceId, 
                                                    request.Width, request.Length, request.Height,
                                                    request.Diameter, request.Thickness);
            
            await _unitOfWork.StockMovements.Add(request);
            
            if (stock != null )
            {
                var oldStock = new Stock{
                    Id = stock.Id,
                    ReferenceId = request.ReferenceId,
                    LocationId = request.LocationId,
                    Quantity = stock.Quantity + request.Quantity,
                    Width = request.Width,
                    Length = request.Length,
                    Height = request.Height,
                    Diameter = request.Diameter,
                    Thickness = request.Thickness 
                };
                await _stockService.Update(oldStock);
            }else{            
                var newStock = new Stock{
                    ReferenceId = request.ReferenceId,
                    LocationId = request.LocationId,
                    Quantity = request.Quantity,
                    Width = request.Width,
                    Length = request.Length,
                    Height = request.Height,
                    Diameter = request.Diameter,
                    Thickness = request.Thickness
                };
                await _stockService.Create(newStock);
            };
            return new GenericResponse(true, request);
        }

         public IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var stockMovements = _unitOfWork.StockMovements.Find(p => p.MovementDate >= startDate && p.MovementDate <= endDate);
            return stockMovements;
        }
    }
}