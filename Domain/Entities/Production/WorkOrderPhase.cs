namespace Domain.Entities.Production;

public class WorkOrderPhase : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public Guid? OperatorTypeId { get; set; }
    public OperatorType? OperatorType { get; set; }
    public Guid? WorkcenterTypeId { get; set; }
    public WorkcenterType? WorkcenterType { get; set; }
    public Guid? PreferredWorkcenterId { get; set; }
    public Workcenter? PreferredWorkcenter { get; set; }

    public bool IsExternalWork { get; set; }
    public decimal ExternalWorkCost { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public Status? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public ICollection<WorkOrderPhaseDetail> Details { get; set; } = new List<WorkOrderPhaseDetail>();
    public ICollection<WorkOrderPhaseBillOfMaterials> BillOfMaterials { get; set; } = new List<WorkOrderPhaseBillOfMaterials>();
}
