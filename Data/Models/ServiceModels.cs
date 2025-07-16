using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LocalGov360.Data.Models
{
    public class ServiceModels
    {
        // Enumerations for field types and validation types
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

        // Entity Models
        public class Service
        {
            public int Id { get; set; }

            [Required]
            [MaxLength(200)]
            public string Name { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            public bool IsActive { get; set; } = true;

            public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

            public DateTime? ModifiedDate { get; set; }

            [MaxLength(100)]
            public string CreatedBy { get; set; }

            [MaxLength(100)]
            public string ModifiedBy { get; set; }

            // Navigation properties
            public virtual ICollection<ServiceField> Fields { get; set; } = new List<ServiceField>();
            public virtual ICollection<ServiceSubmission> Submissions { get; set; } = new List<ServiceSubmission>();
        }

        public class ServiceField
        {
            public int Id { get; set; }

            public int ServiceId { get; set; }

            [Required]
            [MaxLength(100)]
            public string Name { get; set; }

            [Required]
            [MaxLength(200)]
            public string Label { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            public FieldType FieldType { get; set; }

            public bool IsRequired { get; set; }

            public int DisplayOrder { get; set; }

            [MaxLength(200)]
            public string DefaultValue { get; set; }

            [MaxLength(500)]
            public string Placeholder { get; set; }

            // JSON column for storing field-specific options (dropdown items, etc.)
            public string OptionsJson { get; set; }

            // JSON column for storing validation rules
            public string ValidationRulesJson { get; set; }

            // JSON column for storing additional field properties
            public string PropertiesJson { get; set; }

            // Navigation properties
            public virtual Service Service { get; set; }
            public virtual ICollection<ServiceSubmissionValue> SubmissionValues { get; set; } = new List<ServiceSubmissionValue>();

            // Helper properties for working with JSON data
            [NotMapped]
            public List<FieldOption> Options
            {
                get => string.IsNullOrEmpty(OptionsJson) ? new List<FieldOption>() : JsonSerializer.Deserialize<List<FieldOption>>(OptionsJson);
                set => OptionsJson = JsonSerializer.Serialize(value);
            }

            [NotMapped]
            public List<ValidationRule> ValidationRules
            {
                get => string.IsNullOrEmpty(ValidationRulesJson) ? new List<ValidationRule>() : JsonSerializer.Deserialize<List<ValidationRule>>(ValidationRulesJson);
                set => ValidationRulesJson = JsonSerializer.Serialize(value);
            }

            [NotMapped]
            public Dictionary<string, object> Properties
            {
                get => string.IsNullOrEmpty(PropertiesJson) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(PropertiesJson);
                set => PropertiesJson = JsonSerializer.Serialize(value);
            }
        }

        public class ServiceSubmission
        {
            public int Id { get; set; }

            public int ServiceId { get; set; }

            public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

            [MaxLength(100)]
            public string SubmittedBy { get; set; }

            [MaxLength(45)]
            public string IpAddress { get; set; }

            [MaxLength(500)]
            public string UserAgent { get; set; }

            public bool IsCompleted { get; set; } = true;

            // Navigation properties
            public virtual Service Service { get; set; }
            public virtual ICollection<ServiceSubmissionValue> Values { get; set; } = new List<ServiceSubmissionValue>();
        }

        public class ServiceSubmissionValue
        {
            public int Id { get; set; }

            public int SubmissionId { get; set; }

            public int FieldId { get; set; }

            public string Value { get; set; }

            // Navigation properties
            public virtual ServiceSubmission Submission { get; set; }
            public virtual ServiceField Field { get; set; }
        }

        // Supporting classes for JSON serialization
        public class FieldOption
        {
            public string Value { get; set; }
            public string Label { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        public class ValidationRule
        {
            public ValidationType Type { get; set; }
            public string Value { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
