
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalGov360.Data.Models
{
    [Table("ServiceInvoices")]
    public class ServiceInvoice
    {
        public ServiceInvoice()
        {
            LineItems = new List<ServiceInvoiceLineItem>();
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServicePaymentId { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime? PaidDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Unpaid";

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public List<ServiceInvoiceLineItem> LineItems { get; set; }
    }

    [Table("ServiceInvoiceLineItems")]
    public class ServiceInvoiceLineItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceInvoiceId { get; set; }

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;

        // Navigation property
        [ForeignKey(nameof(ServiceInvoiceId))]
        public ServiceInvoice ServiceInvoice { get; set; }
    }
}
