using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalGov360.Data.Models
{
    [Table("ServicePayments")]
    public class ServicePaymentsModels
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowStepId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? OrganisationId { get; set; } // <-- Fix here

        public string PaymentMethod { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;

        public string TransactionReference { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }

        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; } // <-- Add this

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }


}
