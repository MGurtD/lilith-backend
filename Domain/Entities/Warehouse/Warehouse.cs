﻿using Domain.Entities.Production;

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

