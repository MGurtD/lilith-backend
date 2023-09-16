using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class Shift : Entity
    {
        public string Name { get; set; } = string.Empty;
        public TimeOnly startTime { get; set; }
        public TimeOnly endTime { get; set; }
    }
}
