using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace LocalGov360.Data.Models
{
    public class ServiceModels
    {
        public enum FieldType
        {
            Text,
            Number,
            Email,
            Password,
            TextArea,
            Select,
            MultiSelect,
            Radio,
            Checkbox,
            Date,
            DateTime,
            File,
            Boolean
        }

        public enum ValidationType
        {
            Required,
            MinLength,
            MaxLength,
            Min,
            Max,
            Regex,
            Email,
            Custom
        }

        public enum FeeType
        {
            Fixed,
            Variable
        }

        public class Service
        {
            public int Id { get; set; }

            [Required]
            [MaxLength(200)]
            public string Name { get; set; } = string.Empty;

            [MaxLength(500)]
            public string Description { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;
            public decimal? ServiceFee { get; set; } // Changed to nullable
            public FeeType FeeType { get; set; } = FeeType.Fixed;
            public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
            public DateTime? ModifiedDate { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [MaxLength(100)]
            public string CreatedBy { get; set; } = string.Empty;

            [MaxLength(100)]
            public string? ModifiedBy { get; set; }
            public Guid? WorkflowTemplateId { get; set; }
            public Guid? DocumentTemplateId { get; set; }
            public Guid? OrganisationId { get; set; }

            [ForeignKey(nameof(OrganisationId))]
            public Organisation Organisation { get; set; } = default!;
            public virtual WorkflowTemplate? WorkflowTemplate { get; set; }
            public virtual DocumentTemplate? DocumentTemplate { get; set; }
            public virtual ICollection<ServiceField> Fields { get; set; } = new List<ServiceField>();
            public virtual ICollection<ServiceSubmission> Submissions { get; set; } = new List<ServiceSubmission>();
        }

        public class ServiceField
        {
            public int Id { get; set; }
            public int ServiceId { get; set; }

            [Required]
            [MaxLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [MaxLength(200)]
            public string Label { get; set; } = string.Empty;

            [MaxLength(500)]
            public string Description { get; set; } = string.Empty;

            public FieldType FieldType { get; set; } = FieldType.Text;
            public bool IsRequired { get; set; }
            public int DisplayOrder { get; set; }

            [MaxLength(200)]
            public string? DefaultValue { get; set; }

            [MaxLength(500)]
            public string Placeholder { get; set; } = string.Empty;

            public string OptionsJson { get; set; } = "[]";
            public string ValidationRulesJson { get; set; } = "[]";
            public string PropertiesJson { get; set; } = "{}";

            public virtual Service Service { get; set; } = default!;
            public virtual ICollection<ServiceSubmissionValue> SubmissionValues { get; set; } = new List<ServiceSubmissionValue>();

            [NotMapped]
            public List<FieldOption> Options
            {
                get => JsonSerializer.Deserialize<List<FieldOption>>(OptionsJson) ?? new List<FieldOption>();
                set => OptionsJson = JsonSerializer.Serialize(value);
            }

            [NotMapped]
            public List<ValidationRule> ValidationRules
            {
                get => JsonSerializer.Deserialize<List<ValidationRule>>(ValidationRulesJson) ?? new List<ValidationRule>();
                set => ValidationRulesJson = JsonSerializer.Serialize(value);
            }

            [NotMapped]
            public Dictionary<string, object> Properties
            {
                get => JsonSerializer.Deserialize<Dictionary<string, object>>(PropertiesJson) ?? new Dictionary<string, object>();
                set => PropertiesJson = JsonSerializer.Serialize(value);
            }
        }

        public class ServiceSubmission
        {
            public int Id { get; set; }
            public int ServiceId { get; set; }
            public Guid? WorkflowInstanceId { get; set; }
            public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

            [MaxLength(100)]
            public string SubmittedBy { get; set; } = string.Empty;

            [MaxLength(45)]
            public string IpAddress { get; set; } = string.Empty;

            [MaxLength(500)]
            public string UserAgent { get; set; } = string.Empty;
            public bool IsCompleted { get; set; } = true;

            public virtual Service Service { get; set; } = default!;
            public virtual ICollection<ServiceSubmissionValue> Values { get; set; } = new List<ServiceSubmissionValue>();
        }

        public class ServiceSubmissionValue
        {
            public int Id { get; set; }
            public int SubmissionId { get; set; }
            public int FieldId { get; set; }
            public string Value { get; set; } = string.Empty;

            public virtual ServiceSubmission Submission { get; set; } = default!;
            public virtual ServiceField Field { get; set; } = default!;
        }

        public class FieldOption
        {
            public string Value { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public bool IsSelected { get; set; } = false;
        }

        public class ValidationRule
        {
            public ValidationType Type { get; set; }
            public string Value { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}


