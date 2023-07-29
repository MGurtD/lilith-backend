using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Sales
{
    public class Reference : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = String.Empty;
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal Price { get; set; } = decimal.Zero;
    }
}
