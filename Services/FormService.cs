using LocalGov360.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static LocalGov360.Data.Models.ServiceModels;

namespace LocalGov360.Services
{
    public interface IServiceService
    {
        Task<Service> CreateServiceAsync(CreateServiceRequest request);
        Task<Service> GetServiceAsync(int formId);
        Task<List<Service>> GetActiveServicesAsync();
        Task<Service> UpdateServiceAsync(int formId, UpdateServiceRequest request);
        Task<bool> DeleteServiceAsync(int formId);
        Task<ServiceSubmission> SubmitServiceAsync(int formId, SubmitServiceRequest request);
        Task<List<ServiceSubmission>> GetServiceSubmissionsAsync(int formId);
        Task<ServiceSubmission> GetSubmissionAsync(int submissionId);
    }
    public class ServiceService : IServiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormValidator _validator;

        public ServiceService(ApplicationDbContext context, IFormValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Service> CreateServiceAsync(CreateServiceRequest request)
        {
            var Service = new Service
            {
                Name = request.Name,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                Fields = request.Fields.Select((field, index) => new ServiceField
                {
                    Name = field.Name,
                    Label = field.Label,
                    Description = field.Description,
                    FieldType = field.FieldType,
                    IsRequired = field.IsRequired,
                    DisplayOrder = index + 1,
                    DefaultValue = field.DefaultValue,
                    Placeholder = field.Placeholder,
                    Options = field.Options ?? new List<FieldOption>(),
                    ValidationRules = field.ValidationRules ?? new List<ValidationRule>(),
                    Properties = field.Properties ?? new Dictionary<string, object>()
                }).ToList()
            };

            _context.Services.Add(Service);
            await _context.SaveChangesAsync();
            return Service;
        }

        public async Task<Service> GetServiceAsync(int ServiceId)
        {
            return await _context.Services
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == ServiceId);
        }

        public async Task<List<Service>> GetActiveServicesAsync()
        {
            return await _context.Services
                .Where(f => f.IsActive)
                .Include(f => f.Fields)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Service> UpdateServiceAsync(int ServiceId, UpdateServiceRequest request)
        {
            var Service = await _context.Services
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == ServiceId);

            if (Service == null) return null;

            Service.Name = request.Name;
            Service.Description = request.Description;
            Service.ModifiedBy = request.ModifiedBy;
            Service.ModifiedDate = DateTime.UtcNow;

            // Update fields
            _context.ServiceFields.RemoveRange(Service.Fields);
            Service.Fields = request.Fields.Select((field, index) => new ServiceField
            {
                ServiceId = ServiceId,
                Name = field.Name,
                Label = field.Label,
                Description = field.Description,
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                DisplayOrder = index + 1,
                DefaultValue = field.DefaultValue,
                Placeholder = field.Placeholder,
                Options = field.Options ?? new List<FieldOption>(),
                ValidationRules = field.ValidationRules ?? new List<ValidationRule>(),
                Properties = field.Properties ?? new Dictionary<string, object>()
            }).ToList();

            await _context.SaveChangesAsync();
            return Service;
        }

        public async Task<bool> DeleteServiceAsync(int ServiceId)
        {
            var Service = await _context.Services.FindAsync(ServiceId);
            if (Service == null) return false;

            _context.Services.Remove(Service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceSubmission> SubmitServiceAsync(int ServiceId, SubmitServiceRequest request)
        {
            var Service = await GetServiceAsync(ServiceId);
            if (Service == null) throw new ArgumentException("Service not found");

            var validationResult = await _validator.ValidateSubmissionAsync(Service, request.Values);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join(", ", validationResult.Errors));
            }

            var submission = new ServiceSubmission
            {
                ServiceId = ServiceId,
                SubmittedBy = request.SubmittedBy,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Values = request.Values.Select(v => new ServiceSubmissionValue
                {
                    FieldId = v.FieldId,
                    Value = v.Value
                }).ToList()
            };

            _context.ServiceSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<List<ServiceSubmission>> GetServiceSubmissionsAsync(int ServiceId)
        {
            return await _context.ServiceSubmissions
                .Where(s => s.ServiceId == ServiceId)
                .Include(s => s.Values)
                .ThenInclude(v => v.Field)
                .OrderByDescending(s => s.SubmittedDate)
                .ToListAsync();
        }

        public async Task<ServiceSubmission> GetSubmissionAsync(int submissionId)
        {
            return await _context.ServiceSubmissions
                .Include(s => s.Service)
                .Include(s => s.Values)
                .ThenInclude(v => v.Field)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }
    }

    public interface IFormValidator
    {
        Task<ValidationResult> ValidateSubmissionAsync(Service service, List<SubmissionValue> values);
    }

    public class FormValidator : IFormValidator
    {
        public async Task<ValidationResult> ValidateSubmissionAsync(Service service, List<SubmissionValue> values)
        {
            var result = new ValidationResult();
            var submissionDict = values.ToDictionary(v => v.FieldId, v => v.Value);

            foreach (var field in service.Fields)
            {
                var hasValue = submissionDict.ContainsKey(field.Id) && !string.IsNullOrEmpty(submissionDict[field.Id]);
                var value = hasValue ? submissionDict[field.Id] : null;

                // Required field validation
                if (field.IsRequired && !hasValue)
                {
                    result.Errors.Add($"{field.Label} is required");
                    continue;
                }

                if (hasValue)
                {
                    // Apply custom validation rules
                    foreach (var rule in field.ValidationRules)
                    {
                        var ruleResult = ValidateRule(rule, value);
                        if (!ruleResult.IsValid)
                        {
                            result.Errors.Add(ruleResult.ErrorMessage ?? $"{field.Label} validation failed");
                        }
                    }
                }
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private ValidationResult ValidateRule(ValidationRule rule, string value)
        {
            switch (rule.Type)
            {
                case ValidationType.MinLength:
                    return new ValidationResult
                    {
                        IsValid = value.Length >= int.Parse(rule.Value),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ValidationType.MaxLength:
                    return new ValidationResult
                    {
                        IsValid = value.Length <= int.Parse(rule.Value),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ValidationType.Email:
                    return new ValidationResult
                    {
                        IsValid = System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ValidationType.Regex:
                    return new ValidationResult
                    {
                        IsValid = System.Text.RegularExpressions.Regex.IsMatch(value, rule.Value),
                        ErrorMessage = rule.ErrorMessage
                    };
                default:
                    return new ValidationResult { IsValid = true };
            }
        }
    }

    public class CreateServiceRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public List<CreateServiceFieldRequest> Fields { get; set; } = new List<CreateServiceFieldRequest>();
    }

    public class CreateServiceFieldRequest
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public FieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public List<FieldOption> Options { get; set; }
        public List<ValidationRule> ValidationRules { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModifiedBy { get; set; }
        public List<CreateServiceFieldRequest> Fields { get; set; } = new List<CreateServiceFieldRequest>();
    }

    public class SubmitServiceRequest
    {
        public string SubmittedBy { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public List<SubmissionValue> Values { get; set; } = new List<SubmissionValue>();
    }

    public class SubmissionValue
    {
        public int FieldId { get; set; }
        public string Value { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
