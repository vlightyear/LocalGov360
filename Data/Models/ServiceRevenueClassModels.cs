using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalGov360.Data.Models
{
    [Table("ServiceRevenueClasses")]
    public class RevenueClass
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SystemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(10)]
        public string? ParentCode { get; set; }

        [StringLength(50)]
        public string? SpecialCode { get; set; }

        [Required]
        public Guid? OrganisationId { get; set; }

        public bool Bills { get; set; } = false;

        public bool SelfService { get; set; } = false;

        public bool ReqUploads { get; set; } = false;

        [StringLength(100)]
        public string? BudgetLine { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("OrganisationId")]
        public virtual Organisation? Organisation { get; set; }
    }
}