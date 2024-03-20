using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class WorkMasterCosts
    {
        public decimal OperatorCost { get; set; }
        public decimal MachineCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal ExternalCost { get; set; }

        public WorkMasterCosts(decimal operatorCost, decimal machineCost, decimal materialCost, decimal externalCost)
        {
            OperatorCost = operatorCost;
            MachineCost = machineCost;
            MaterialCost = materialCost;
            ExternalCost = externalCost;
        }
    }
}
