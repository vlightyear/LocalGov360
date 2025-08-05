using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace LocalGov360.Services
{
    public interface IServiceService
    {
        Task<ServiceModels.Service> CreateServiceAsync(CreateServiceRequest request);
        Task<ServiceModels.Service> GetServiceAsync(int serviceId);
        Task<List<ServiceModels.Service>> GetActiveServicesAsync();
        Task<ServiceModels.Service> UpdateServiceAsync(int serviceId, UpdateServiceRequest request);
        Task<bool> DeleteServiceAsync(int serviceId);
        Task<ServiceModels.ServiceSubmission> SubmitServiceAsync(int serviceId, SubmitServiceRequest request);
        Task<List<ServiceModels.ServiceSubmission>> GetServiceSubmissionsAsync(int serviceId);
        Task<ServiceModels.ServiceSubmission> GetSubmissionAsync(int submissionId);
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

        public async Task<ServiceModels.Service> CreateServiceAsync(CreateServiceRequest request)
        {
            var service = new ServiceModels.Service
            {
                Name = request.Name,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                OrganisationId = request.OrganisationId,
                WorkflowTemplateId = request.WorkflowTemplateId,
                DocumentTemplateId = request.DocumentTemplateId,
                ServiceFee = request.ServiceFee, // Now nullable
                FeeType = request.FeeType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Fields = request.Fields.Select((field, index) => new ServiceModels.ServiceField
                {
                    Name = !string.IsNullOrWhiteSpace(field.Name) ? field.Name : field.Label,
                    Label = field.Label,
                    Description = field.Description ?? string.Empty,
                    FieldType = field.FieldType,
                    IsRequired = field.IsRequired,
                    DisplayOrder = index + 1,
                    DefaultValue = field.DefaultValue ?? string.Empty,
                    Placeholder = field.Placeholder ?? string.Empty,
                    Options = field.Options?.Count > 0 ? field.Options : new List<ServiceModels.FieldOption>(),
                    ValidationRules = field.ValidationRules?.Count > 0 ? field.ValidationRules : new List<ServiceModels.ValidationRule>(),
                    Properties = field.Properties?.Count > 0 ? field.Properties : new Dictionary<string, object>()
                }).ToList()
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<ServiceModels.Service> GetServiceAsync(int serviceId)
        {
            return await _context.Services
                .Include(f => f.Fields)
                .Include(f => f.WorkflowTemplate)
                .Include(f => f.DocumentTemplate)
                .FirstOrDefaultAsync(f => f.Id == serviceId);
        }

        public async Task<List<ServiceModels.Service>> GetActiveServicesAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Services
                .Where(f => f.IsActive &&
                           (!f.StartDate.HasValue || f.StartDate <= currentDate) &&
                           (!f.EndDate.HasValue || f.EndDate >= currentDate))
                .Include(f => f.Fields)
                .Include(f => f.WorkflowTemplate)
                .Include(f => f.DocumentTemplate)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<ServiceModels.Service> UpdateServiceAsync(int serviceId, UpdateServiceRequest request)
        {
            var service = await _context.Services
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == serviceId);

            if (service == null) return null;

            service.Name = request.Name;
            service.Description = request.Description;
            service.ModifiedBy = request.ModifiedBy;
            service.ModifiedDate = DateTime.UtcNow;
            service.WorkflowTemplateId = request.WorkflowTemplateId;
            service.DocumentTemplateId = request.DocumentTemplateId;
            service.ServiceFee = request.ServiceFee; // Now nullable
            service.FeeType = request.FeeType;
            service.StartDate = request.StartDate;
            service.EndDate = request.EndDate;

            _context.ServiceFields.RemoveRange(service.Fields);
            service.Fields = request.Fields.Select((field, index) => new ServiceModels.ServiceField
            {
                ServiceId = serviceId,
                Name = field.Name,
                Label = field.Label,
                Description = field.Description,
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                DisplayOrder = index + 1,
                DefaultValue = field.DefaultValue ?? string.Empty,
                Placeholder = field.Placeholder,
                Options = field.Options ?? new List<ServiceModels.FieldOption>(),
                ValidationRules = field.ValidationRules ?? new List<ServiceModels.ValidationRule>(),
                Properties = field.Properties ?? new Dictionary<string, object>()
            }).ToList();

            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<bool> DeleteServiceAsync(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceModels.ServiceSubmission> SubmitServiceAsync(int serviceId, SubmitServiceRequest request)
        {
            var service = await GetServiceAsync(serviceId);
            if (service == null) throw new ArgumentException("Service not found");

            var validationResult = await _validator.ValidateSubmissionAsync(service, request.Values);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join(", ", validationResult.Errors));
            }

            var submission = new ServiceModels.ServiceSubmission
            {
                ServiceId = serviceId,
                SubmittedBy = request.SubmittedBy,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Values = request.Values.Select(v => new ServiceModels.ServiceSubmissionValue
                {
                    FieldId = v.FieldId,
                    Value = v.Value
                }).ToList()
            };

            _context.ServiceSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<List<ServiceModels.ServiceSubmission>> GetServiceSubmissionsAsync(int serviceId)
        {
            return await _context.ServiceSubmissions
                .Where(s => s.ServiceId == serviceId)
                .Include(s => s.Values)
                .ThenInclude(v => v.Field)
                .OrderByDescending(s => s.SubmittedDate)
                .ToListAsync();
        }

        public async Task<ServiceModels.ServiceSubmission> GetSubmissionAsync(int submissionId)
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
        Task<ValidationResult> ValidateSubmissionAsync(ServiceModels.Service service, List<SubmissionValue> values);
    }

    public class FormValidator : IFormValidator
    {
        public async Task<ValidationResult> ValidateSubmissionAsync(ServiceModels.Service service, List<SubmissionValue> values)
        {
            var result = new ValidationResult();
            var submissionDict = values.ToDictionary(v => v.FieldId, v => v.Value);

            foreach (var field in service.Fields)
            {
                var hasValue = submissionDict.ContainsKey(field.Id) && !string.IsNullOrEmpty(submissionDict[field.Id]);
                var value = hasValue ? submissionDict[field.Id] : null;

                if (field.IsRequired && !hasValue)
                {
                    result.Errors.Add($"{field.Label} is required");
                    continue;
                }

                if (hasValue)
                {
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

        private ValidationResult ValidateRule(ServiceModels.ValidationRule rule, string value)
        {
            switch (rule.Type)
            {
                case ServiceModels.ValidationType.MinLength:
                    return new ValidationResult
                    {
                        IsValid = value.Length >= int.Parse(rule.Value),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ServiceModels.ValidationType.MaxLength:
                    return new ValidationResult
                    {
                        IsValid = value.Length <= int.Parse(rule.Value),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ServiceModels.ValidationType.Email:
                    return new ValidationResult
                    {
                        IsValid = System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                        ErrorMessage = rule.ErrorMessage
                    };
                case ServiceModels.ValidationType.Regex:
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
        public Guid? OrganisationId { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public Guid? WorkflowTemplateId { get; set; }
        public Guid? DocumentTemplateId { get; set; }
        public decimal? ServiceFee { get; set; } // Changed to nullable
        public ServiceModels.FeeType FeeType { get; set; } = ServiceModels.FeeType.Fixed;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CreateServiceFieldRequest> Fields { get; set; } = new List<CreateServiceFieldRequest>();
    }

    public class CreateServiceFieldRequest
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public ServiceModels.FieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public List<ServiceModels.FieldOption> Options { get; set; }
        public List<ServiceModels.ValidationRule> ValidationRules { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string Name { get; set; }
        public Guid? OrganisationId { get; set; }
        public string Description { get; set; }
        public string ModifiedBy { get; set; }
        public Guid? WorkflowTemplateId { get; set; }
        public Guid? DocumentTemplateId { get; set; }
        public decimal? ServiceFee { get; set; } // Changed to nullable
        public ServiceModels.FeeType FeeType { get; set; } = ServiceModels.FeeType.Fixed;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
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