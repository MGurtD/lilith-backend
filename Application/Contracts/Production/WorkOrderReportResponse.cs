using Application.Services;

namespace Application.Contracts.Production;

public class WorkOrderReportResponse : ReportResponse
{
    public required WorkOrderReportDto Order { get; set; }
    public required List<WorkOrderPhaseReportDto> Phases { get; set; }

    public WorkOrderReportResponse() : base() { }
}

public class WorkOrderReportDto
{
    public required string Code { get; set; }
    public required string ReferenceCode { get; set; }
    public required string ReferenceDescription { get; set; }
    public required DateTime PlannedDate { get; set; }
    public required decimal PlannedQuantity { get; set; }
    public required decimal TotalQuantity { get; set; }
    public required string StatusName { get; set; }
    public required decimal OperatorCost { get; set; }
    public required decimal MachineCost { get; set; }
    public required decimal MaterialCost { get; set; }
    public required decimal TotalCost { get; set; }
}

public class WorkOrderPhaseReportDto
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public required string StatusName { get; set; }
    public required decimal OperatorTime { get; set; }
    public required decimal MachineTime { get; set; }
    public required decimal ExternalWorkCost { get; set; }
    public required decimal TransportCost { get; set; }
    public required List<WorkOrderPhaseDetailReportDto> Details { get; set; }
    public required List<WorkOrderPhaseBillOfMaterialsReportDto> BillOfMaterials { get; set; }
}

public class WorkOrderPhaseDetailReportDto
{
    public required string Description { get; set; }
    public required string WorkcenterTypeName { get; set; }
    public required string OperatorTypeName { get; set; }
    public required decimal EstimatedTime { get; set; }
    public required decimal RealTime { get; set; }
}

public class WorkOrderPhaseBillOfMaterialsReportDto
{
    public required string ReferenceCode { get; set; }
    public required string ReferenceDescription { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal Width { get; set; }
    public required decimal Length { get; set; }
    public required decimal Thickness { get; set; }
    public required decimal Diameter { get; set; }
}
