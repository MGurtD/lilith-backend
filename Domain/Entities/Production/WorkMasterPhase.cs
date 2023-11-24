using Domain.Entities;

namespace Domain.Entities.Production;
public class WorkMasterPhase : Entity
{
    public string PhaseCode {get; set;} = string.Empty;

    public string PhaseDescription {get; set;} = string.Empty;

    public Guid WorkMasterId {get; set;}

    public WorkMaster? WorkMaster {get; set;}
}
