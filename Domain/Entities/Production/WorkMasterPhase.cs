using Domain.Entities;
using Domain.Entities.Sales;

namespace Domain.Entities.Production;
public class WorkMasterPhase : Entity
{
    public string PhaseCode {get; set;} = string.Empty;
    public string PhaseDescription {get; set;} = string.Empty;
    public Guid WorkMasterId {get; set;}
    public WorkMaster? WorkMaster {get; set;}

    public ICollection<WorkMasterPhaseDetail> Details { get; set; } = new List<WorkMasterPhaseDetail>();
    public ICollection<WorkMasterPhaseBillOfMaterials> BillOfMaterials { get; set; } = new List<WorkMasterPhaseBillOfMaterials>();
}
