using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Purchase
{
    public class PurchaseOrderFromWO
    {
        [Required]
        public Guid workorderId { get; set; }
        [Required]
        public string workorderDescription {  get; set; }
        [Required]
        public Guid phaseId { get; set; }
        [Required]
        public string phaseDescription { get; set; }
        [Required]
        public Guid serviceReferenceId { get; set; }
        [Required]
        public string serviceReferenceName { get; set; }
        [Required]
        public Guid supplierId { get; set; }
        [Required]
        public int quantity {  get; set; }
    }
}
