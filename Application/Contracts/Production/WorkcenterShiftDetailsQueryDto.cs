using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Contracts.Production;

public class WorkcenterShiftDetailsQueryDto
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public Guid? WorkcenterId { get; set; }

    public Guid? OperatorId { get; set; }

    public Guid? WorkOrderPhaseId { get; set; }

    public GroupBy? GroupBy { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupBy
{
    None,
    Workcenter,
    Operator,
    WorkOrderPhase
}

public class WorkcenterShiftDetailResponseDto
{
    public Guid Id { get; set; }
    public Guid WorkcenterShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool Current { get; set; }

    // Machine Status
    public Guid MachineStatusId { get; set; }
    public string? MachineStatusName { get; set; }

    // Workcenter
    public Guid WorkcenterId { get; set; }
    public string? WorkcenterName { get; set; }
    public decimal WorkcenterCost { get; set; }

    // Operator
    public Guid? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public decimal OperatorCost { get; set; }
    public int ConcurrentOperatorWorkcenters { get; set; }

    // Work Order Phase
    public Guid? WorkOrderPhaseId { get; set; }
    public string? WorkOrderPhaseName { get; set; }
    public string? WorkOrderCode { get; set; }
    public int ConcurrentWorkorderPhases { get; set; }

    // Quantities
    public decimal QuantityOk { get; set; }
    public decimal QuantityKo { get; set; }

    // Calculated
    public decimal TotalHours { get; set; }
    public decimal TotalCost { get; set; }
}

public class GroupedWorkcenterShiftDetailsDto
{
    public string GroupKey { get; set; } = string.Empty;
    public Guid? GroupId { get; set; }
    public List<WorkcenterShiftDetailResponseDto> Details { get; set; } = [];

    // Aggregated data
    public decimal TotalHours { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalQuantityOk { get; set; }
    public decimal TotalQuantityKo { get; set; }
    public int DetailCount { get; set; }
}