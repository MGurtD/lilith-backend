using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Production;

public class WorkOrderPhaseInOutRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid WorkOrderPhaseId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}
