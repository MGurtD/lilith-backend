using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Production;

public class WorkcenterShift : Entity
{
    public Guid WorkcenterId { get; set; }
    public Workcenter? Workcenter { get; set; }
    public Guid ShiftDetailId { get; set; }
    public ShiftDetail? ShiftDetail { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ICollection<WorkcenterShiftDetail> Details { get; set; } = [];

    [NotMapped]
    public override DateTime CreatedOn { get => base.CreatedOn; set => base.CreatedOn = value; }
    [NotMapped]
    public override DateTime UpdatedOn { get => base.UpdatedOn; set => base.UpdatedOn = value; }
}
