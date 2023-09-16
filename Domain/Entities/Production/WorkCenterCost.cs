using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class WorkCenterCost : Entity
    {
        public Guid WorkcenterId { get; set; }
        public Workcenter? Workcenter { get; set; }
        public Guid MachineStatusId { get; set; }
        public MachineStatus? MachineStatus { get; set; }

        public decimal Cost { get; set; }
    }
}
