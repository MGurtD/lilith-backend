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
            var materailCost = 0.0M;
            var resp = false;
            
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
                    var cost = (workCenterCost.Cost == null) ? 0.0M : workCenterCost.Cost;
                    
                        operatorCost += (phaseTime / 60) * operatorType.Cost;
                        machineCost += (phaseTime / 60) * cost;            

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
                    var referenceType = await _unitOfWork.ReferenceTypes.Get((Guid)reference.ReferenceTypeId);


                    // Obtenir calculadora segons el format
                    var format = await _unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                    var dimensionsCalculator = ReferenceFormatCalculationFactory.Create(format!.Code);
                    // Assignar dimensions a la calculadoras
                    var dimensions = new ReferenceDimensions();
                    /*Quantity = receiptDetail.Quantity;
                    Width = receiptDetail.Width;
                    Length = receiptDetail.Lenght;
                    Height = receiptDetail.Height;
                    Diameter = receiptDetail.Diameter;
                    Thickness = receiptDetail.Thickness;
                    */
                    dimensions.Quantity = (int)bom.Quantity;
                    dimensions.Width = bom.Width;
                    dimensions.Length = bom.Length;
                    dimensions.Height = bom.Height;
                    dimensions.Diameter = bom.Diameter;
                    dimensions.Thickness = bom.Thickness;
                    dimensions.Density = referenceType!.Density;

                    // Calcular el pes
                    var UnitWeight = Math.Round(dimensionsCalculator.Calculate(dimensions), 2);
                    var TotalWeight = UnitWeight * bom.Quantity;
                    // Calcular el preu
                    var UnitPrice = reference.LastPurchaseCost * UnitWeight;
                    var Amount = reference.LastPurchaseCost * TotalWeight;
                    materailCost += Amount;
                }
            }
            //
            result = operatorCost + machineCost + materailCost;
            if(result > 0)
            {
                resp = true;
            }
            return new GenericResponse(resp, result);
        }
    }
}
