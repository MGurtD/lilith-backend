namespace Application.Contracts.Production
{
    public class CreateWorkOrderDto
    {
        public Guid WorkMasterId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public DateTime PlannedDate { get; set; }
        public Guid? SalesOrderId { get; set; }
        public Guid? SalesOrderDetailId { get; set; }
        public string Comment { get; set; } = string.Empty;
}
}
