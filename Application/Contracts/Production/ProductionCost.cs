using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Production
{
    public  class ProductionCost : Contract
    {
        public DateTime Date { get; set; }
        public double Year { get; set; }
        public double Month { get; set; }
        public double Week { get; set; }
        public decimal WorkcenterTime { get; set; }
        public decimal MachineHourCost { get; set; }
        public decimal PartWorkcenterCost { get; set; }
        public decimal OperatorTime { get; set; }
        public decimal OperatorHourCost { get; set; }
        public decimal PartOperatorCost { get; set; }
        public string WorkcenterName { get; set; }
        public string WorkcenterTypeName { get; set; }
    }
}
