namespace Domain.Entities.Production;

public class WorkMasterPhase : Entity
{
    public string PhaseCode {get; set;} = string.Empty;
    public string PhaseDescription {get; set;} = string.Empty;
    public Guid WorkMasterId {get; set;}
    public WorkMaster? WorkMaster {get; set;}

    public Guid WorkcenterTypeId { get; set; }
    public WorkcenterType? WorkcenterType { get; set; }
    public Guid? PreferredWorkcenterId { get; set; }
    public Workcenter? PreferredWorkcenter { get; set; }
    public Guid OperatorTypeId { get; set; }
    public OperatorType? OperatorType { get; set; }

    public ICollection<WorkMasterPhaseDetail> Details { get; set; } = new List<WorkMasterPhaseDetail>();
    public ICollection<WorkMasterPhaseBillOfMaterials> BillOfMaterials { get; set; } = new List<WorkMasterPhaseBillOfMaterials>();
}
