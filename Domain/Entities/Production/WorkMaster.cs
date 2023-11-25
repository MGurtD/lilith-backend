using Domain.Entities;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkMaster : Entity
{
    public Guid ReferenceId { get; set;}
    public Reference? Reference { get; set;}
    public decimal BaseQuantity { get; set;}

    public ICollection<WorkMasterPhase> Phases { get; set; } = new List<WorkMasterPhase>();
}
