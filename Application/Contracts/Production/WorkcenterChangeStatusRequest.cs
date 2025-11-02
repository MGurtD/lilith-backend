using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Production;

public class WorkcenterChangeStatusRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid MachineStatusId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}
