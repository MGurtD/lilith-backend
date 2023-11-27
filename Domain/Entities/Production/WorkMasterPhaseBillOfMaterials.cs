using Domain.Entities;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkMasterPhaseBillOfMaterials: Entity
{
    public Guid WorkMasterPhaseId {get; set;}
    public WorkMasterPhase? WorkMasterPhase {get; set;}
    public Guid ReferenceId {get; set;}
    public Reference? Reference {get; set;}
    public decimal Quantity {get; set;}
    public Guid? WasteReferenceId {get;set;}
    public Reference? WasteReference {get; set;}
    public decimal Waste {get;set;}
}
