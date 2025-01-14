using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Sales
{
    public class Revenue : Contract
    {
        public double Year { get; set; }
        public double Month { get; set; }
        public double OutcomeAmount { get; set; }
        public double ExpenseAmount { get; set; }
        public double IncomeAmount { get; set; }
        public double RevenueAmount { get; set; }
    }
}
