using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public interface ICostsService
    {
        Task<GenericResponse> GetOperatorCost(Guid operatorId);
        Task<GenericResponse> GetWorkcenterStatusCost(Guid workcenterId, Guid statusId);

        Task<GenericResponse> GetWorkmasterCost(WorkMaster workMaster);
        Task<GenericResponse> GetProductionPartCosts(ProductionPart productionPart);
    }
}
