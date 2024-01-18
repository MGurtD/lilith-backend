using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class WorkMasterCopy
    {
        public Guid? ReferenceId { get; set; }
        public Guid WorkmasterId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
