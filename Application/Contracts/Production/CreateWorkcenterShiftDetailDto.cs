using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Production;

public class CreateWorkcenterShiftDetailDto
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid MachineStatusId { get; set; }
    public Guid? OperatorId { get; set; }
    public Guid? WorkOrderPhaseId { get; set; }
}
