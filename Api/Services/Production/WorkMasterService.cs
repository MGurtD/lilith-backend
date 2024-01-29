using Application.Contracts;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;

namespace Api.Services.Production
{
    public class WorkMasterService : IWorkMasterService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkMasterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<GenericResponse> Calculate(WorkMaster workMaster)
        {
            var result decimal = 0.0M;
            //Recollir la quantitat base
            var baseQuantity = workMaster.BaseQuantity;
            //Recorrer les phases
            //A cada fase, recollir el operatortypeId, i buscar el seu preu cost/hora
            //Per cada fase recorrer els detalls i obtenim el temps+estat maquina, es busca el cost del binomi
            //Si es temps de cicle es multiplica per la quantitat base, sinó es el temps del bloc. Es multiplica el temps pel cost
            //temps en minuts cost en hores
            foreach(var phase in workMaster.Phases)
            {
                phase.
            }
            //

            return new GenericResponse(false, result);
        }
    }
}
