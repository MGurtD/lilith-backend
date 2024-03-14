using Application.Contracts;
using Application.Persistance;
using Application.Services.Production;
using Domain.Entities.Production;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Api.Services.Production
{
    public class WorkMasterService : IWorkMasterService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkMasterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GenericResponse> Calculate(WorkMaster workMaster)
        {
            var result = 0.0M;
            //Recollir la quantitat base
            var baseQuantity = workMaster.BaseQuantity;
            var operatorCost = 0.0M;
            var machineCost = 0.0M;
            var materialCost = 0.0M;
            var externalCost = 0.0M;
            
            //Recorrer les phases
            //A cada fase, recollir el operatortypeId, i buscar el seu preu cost/hora
            //Per cada fase recorrer els detalls i obtenim el temps+estat maquina, es busca el cost del binomi
            //Si es temps de cicle es multiplica per la quantitat base, sinó es el temps del bloc. Es multiplica el temps pel cost
            //temps en minuts cost en hores
            foreach(var phase in workMaster.Phases)
            {
                var operatorType = _unitOfWork.OperatorTypes.Find(o => o.Id == phase.OperatorTypeId).FirstOrDefault();

                var phaseDetails = await _unitOfWork.WorkMasters.Phases.Get(phase.Id);
                if(phaseDetails == null) continue;

                // Cálcul de fases externes
                if (phase.IsExternalWork) {
                    externalCost += phase.ExternalWorkCost;
                    continue;
                }

                //Temps
                foreach (var detail in phaseDetails.Details)
                {                                          
                    var phaseTime = 0.0M;
                    if (detail.IsCycleTime)
                    {
                        phaseTime = baseQuantity * detail.EstimatedTime;
                    }
                    else
                    {
                        phaseTime = detail.EstimatedTime;
                    }
                    //Cost Màquina                    
                    var workCenterCost = _unitOfWork.WorkcenterCosts.Find(wc => wc.WorkcenterId == phase.PreferredWorkcenterId && wc.MachineStatusId == detail.MachineStatusId).FirstOrDefault();
                    if (workCenterCost == null) {
                        return new GenericResponse(false, "No s'ha trobat la combinació de centre de treball i estat de màquina");
                    }

                    var cost = workCenterCost.Cost;
                    
                    operatorCost += phaseTime / 60 * (operatorType != null ? operatorType.Cost : 0);
                    machineCost += phaseTime / 60 * cost;       
                }
                //Material
                foreach (var bom in phaseDetails.BillOfMaterials)
                {
                    
                    var reference = await _unitOfWork.References.Get(bom.ReferenceId);
                    if(reference == null) continue;
                    if (!reference.ReferenceTypeId.HasValue)
                    {
                        return new GenericResponse(false, "No s'ha trobat el tipus de material");
                    }

                    if (reference.ReferenceFormatId.HasValue) {
                        var referenceType = await _unitOfWork.ReferenceTypes.Get((Guid)reference.ReferenceTypeId);

                        // Obtenir calculadora segons el format
                        var format = await _unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                        var dimensionsCalculator = ReferenceFormatCalculationFactory.Create(format!.Code);

                        // Assignar dimensions a la calculadoras
                        var dimensions = new ReferenceDimensions
                        {
                            Quantity = (int)bom.Quantity,
                            Width = bom.Width,
                            Length = bom.Length,
                            Height = bom.Height,
                            Diameter = bom.Diameter,
                            Thickness = bom.Thickness,
                            Density = referenceType!.Density
                        };

                        // Calcular el pes
                        var UnitWeight = Math.Round(dimensionsCalculator.Calculate(dimensions), 2);
                        var TotalWeight = UnitWeight * bom.Quantity;
                        // Calcular el preu
                        var UnitPrice = reference.LastCost * UnitWeight;
                        var Amount = reference.LastCost * TotalWeight;
                        materialCost += Amount;
                    }

                }
            }
            var costDetails = new WorkMasterCosts(operatorCost, machineCost, materialCost, externalCost);

            //result = operatorCost + machineCost + materialCost + externalCost;
            return new GenericResponse(true, costDetails);
        }
    }
}
