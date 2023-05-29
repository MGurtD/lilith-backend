using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CustomerTypeDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public bool Disabled { get; set; } = false;
        
    }
}
