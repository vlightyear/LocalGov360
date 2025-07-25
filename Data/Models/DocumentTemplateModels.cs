namespace LocalGov360.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DocumentTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [StringLength(100, ErrorMessage = "Template name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Template content is required")]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public Guid? OrganisationId { get; set; }

        [ForeignKey(nameof(OrganisationId))]
        public Organisation Organisation { get; set; } = default!;
    }
}