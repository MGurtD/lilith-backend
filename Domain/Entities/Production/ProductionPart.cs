﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class ProductionPart : Entity
    {
        public DateTime Date { get; set; }
        public Guid WorkcenterId { get; set; }
        public Workcenter? Workcenter { get; set; }
        public Guid WorkOrderPhaseDetailId { get; set; }
        public WorkOrderPhaseDetail? WorkOrderPhaseDetail { get; set;}
        public Guid OperatorId { get; set; }
        public Operator? Operator { get; set; }
        public int Quantity { get; set; }
        public decimal Time { get; set; }
    }
}
