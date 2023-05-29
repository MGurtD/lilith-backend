using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerType : Entity
    {
        public string Name { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
    }
}
