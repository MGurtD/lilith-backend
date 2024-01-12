using Domain.Entities.Sales;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkOrder : Entity
{
    public string Code { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public Reference? Reference { get; set;}
    public Guid WorkMasterId { get; set; }
    public WorkMaster? WorkMaster { get; set; }
    public Guid StatusId { get; set; }
    public Status? Status { get; set; }
    public Guid? SalesOrderHeaderId { get; set; }
    public SalesOrderHeader? SalesOrderHeader { get; set; }
    public Guid? SalesOrderDetailId { get; set; }
    public SalesOrderDetail? SalesOrderDetail { get; set; }

    public decimal PlannedQuantity { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Order { get; set; }
    public string Comment { get; set; } = string.Empty;

    public ICollection<WorkOrderPhase> Phases { get; set; } = new List<WorkOrderPhase>();
}
