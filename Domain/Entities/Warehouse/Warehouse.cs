using Domain.Entities.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Warehouse
{
    public class Warehouse : Entity
    {
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid SiteId { get; set; }
        public Site? Site { get; set; }
    }
}

