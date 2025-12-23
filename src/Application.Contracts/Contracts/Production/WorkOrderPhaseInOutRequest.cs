using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class WorkOrderPhaseInOutRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid WorkOrderPhaseId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}
