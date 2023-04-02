﻿namespace Domain.Entities
{
    public class Enterprise : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}
