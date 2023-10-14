using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Warehouse;

namespace Api.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<GenericResponse> Create(Stock request)
        {
            await _unitOfWork.Stocks.Add(request);
            return new GenericResponse(true, request);
        }

        public async Task<GenericResponse> Update(Stock request)
        {
            await _unitOfWork.Stocks.Update(request);
            return new GenericResponse(true);
        }

        public IEnumerable<Stock> GetByLocation(Guid locationId)
        {
            var stocks = _unitOfWork.Stocks.Find(p => p.LocationId == locationId)
                            .GroupBy(p => new { p.Width, p.Length, p.Height, p.Diameter, p.Thickness})
                            .Select(b => new {  Width = b.Key.Width, 
                                                Length = b.Key.Length,
                                                Height = b.Key.Height,
                                                Diameter = b.Key.Diameter,
                                                Thickness = b.Key.Thickness,
                                                Quantity = b.Sum(x => x.Quantity)});
            return (IEnumerable<Stock>)stocks;
        }

        public IEnumerable<Stock> GetByReference(Guid referenceId)
        {
            var stocks = _unitOfWork.Stocks.Find(p => p.ReferenceId == referenceId)
                            .GroupBy(p => new { p.Width, p.Length, p.Height, p.Diameter, p.Thickness})
                            .Select(b => new {  Width = b.Key.Width, 
                                                Length = b.Key.Length,
                                                Height = b.Key.Height,
                                                Diameter = b.Key.Diameter,
                                                Thickness = b.Key.Thickness,
                                                Quantity = b.Sum(x => x.Quantity)});
            return (IEnumerable<Stock>)stocks;
        }

        public IEnumerable<Stock> GetAll()
        {
            var stocks =  _unitOfWork.Stocks.GetAll();
            return (IEnumerable<Stock>)stocks;
        }
        /*public IEnumerable<Stock> GetByReferenceType(Guid referenceTypeId)
        {
            var stocks = _unitOfWork.Stocks.
        }*/


    }
}