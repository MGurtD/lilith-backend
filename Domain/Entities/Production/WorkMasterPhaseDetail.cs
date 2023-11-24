using Domain.Entities;
using Domain.Entities.Production;

namespace Domain.Entities.Production;
public class WorkMasterPhaseDetail : Entity
{
    public Guid WorkMasterPhaseId {get; set;}
    public WorkMasterPhase? WorkMasterPhase { get; set;}
    public Guid WorkcenterTypeId {get; set;}
    public WorkcenterType? WorkcenterType {get; set;}
    public Guid PreferredWorkcenterId {get; set;}
    public Workcenter? PreferredWorkcenter {get; set;}
    public Guid OperatorTypeId {get; set;}
    public OperatorType? OperatorType {get; set;}
    public Guid MachineStatusId {get; set;}
    public MachineStatus? MachineStatus{get; set;}
    public decimal EstimatedTime {get; set;}
    public bool IsCycleTime {get;set;}
    public bool IsExternalWork {get; set;}
    public decimal ExternalWorkCost {get; set; }
    public Guid? PurchaseOrderId {get; set;}
    
}
