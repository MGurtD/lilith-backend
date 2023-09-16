using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class OperatorCost : Entity
    {
        public Guid OperatorTypeId { get; set; }
        public OperatorType? OperatorType { get; set; }
        public Guid MachineStatusId { get; set; }
        public MachineStatus? MachineStatus { get; set; }
        public decimal Cost { get; set; }
    }
}
