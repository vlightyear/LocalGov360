using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalGov360.Data.Models
{
    /// <summary>
    /// Main valuation roll document
    /// </summary>
    [Table("CouncilValuationRolls")]
    public class ValuationRoll
    {
        [Key]
        public int Id { get; set; }

        public Guid? OrganisationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Council { get; set; } = string.Empty;

        [Required]
        public DateTime ValuationDate { get; set; }

        [Required]
        public int Year { get; set; }

        [StringLength(50)]
        public string? RollNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
        public virtual ICollection<PoundageRate> PoundageRates { get; set; } = new List<PoundageRate>();
        public virtual ICollection<PropertyEvaluation> PropertyEvaluations { get; set; } = new List<PropertyEvaluation>();
    }

    /// <summary>
    /// Property types with their details
    /// </summary>
    [Table("CouncilPropertyTypes")]
    public class PropertyType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string ShortName { get; set; } = string.Empty; // RES, COM, etc.

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Residential, Commercial, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
        public virtual ICollection<PoundageRate> PoundageRates { get; set; } = new List<PoundageRate>();
    }

    /// <summary>
    /// Poundage rates for different property types and periods
    /// </summary>
    [Table("CouncilPoundageRates")]
    public class PoundageRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropertyTypeId { get; set; }

        public int? ValuationRollId { get; set; } // Optional - can be council-wide or roll-specific

        public Guid? OrganisationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal Rate { get; set; } // e.g., 0.001, 0.002

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsCurrent { get; set; } = true;

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("PropertyTypeId")]
        public virtual PropertyType PropertyType { get; set; } = null!;

        [ForeignKey("ValuationRollId")]
        public virtual ValuationRoll? ValuationRoll { get; set; }
    }

    /// <summary>
    /// Individual property in the valuation roll
    /// </summary>
    [Table("CouncilProperties")]
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ValuationRollId { get; set; }

        [Required]
        public int PropertyTypeId { get; set; } // Foreign key to PropertyType

        [Required]
        [StringLength(50)]
        public string PropertyNumber { get; set; } = string.Empty; // CHONG/0001

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string? StreetAddress { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? Leaseholder { get; set; }

        [Required]
        [StringLength(10)]
        public string Use { get; set; } = "RES"; // Keeping for backward compatibility

        [Column(TypeName = "decimal(10,4)")]
        public decimal? LandExtHa { get; set; } // Land Ext (HA)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LandValue { get; set; } // Land Value (K)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValueOfImprovements { get; set; } // Value of Improvements (K)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRateableValue { get; set; } // Total Rateable Value (K)

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Poundage { get; set; } // Calculated poundage amount

        [Column(TypeName = "decimal(10,6)")]
        public decimal? PoundageRate { get; set; } // Rate used for calculation

        // New field for evaluation amount
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EvaluationAmount { get; set; } // Calculated evaluation amount

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("ValuationRollId")]
        public virtual ValuationRoll ValuationRoll { get; set; } = null!;

        [ForeignKey("PropertyTypeId")]
        public virtual PropertyType PropertyType { get; set; } = null!;

        public virtual ICollection<PropertyEvaluation> PropertyEvaluations { get; set; } = new List<PropertyEvaluation>();
    }

    /// <summary>
    /// Property evaluation records for bi-annual billing
    /// </summary>
    [Table("CouncilPropertyEvaluations")]
    public class PropertyEvaluation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int ValuationRollId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Period { get; set; } // 1 = Jan-June, 2 = July-December

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RateableValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal PoundageRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EvaluationAmount { get; set; } // RateableValue * PoundageRate / 2

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; }

        public DateTime? PaymentDate { get; set; }

        public bool IsPaid { get; set; } = false;

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = null!;

        [ForeignKey("ValuationRollId")]
        public virtual ValuationRoll ValuationRoll { get; set; } = null!;
    }

    /// <summary>
    /// Simple DTO for importing property data
    /// </summary>
    public class PropertyImport
    {
        public string? PropertyNumber { get; set; }
        public string? Description { get; set; }
        public string? StreetAddress { get; set; }
        public string? Location { get; set; }
        public string? Leaseholder { get; set; }
        public string? Use { get; set; }
        public string? LandExtHa { get; set; }
        public string? LandValueK { get; set; }
        public string? ValueOfImprovementsK { get; set; }
        public string? TotalRateableValueK { get; set; }
        public string? Remarks { get; set; }

        // Helper properties
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();
        public int RowNumber { get; set; }
    }

    /// <summary>
    /// Service result classes
    /// </summary>
    public class ValuationUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int ValuationRollId { get; set; }
        public int TotalRowsProcessed { get; set; }
        public int SuccessfulRows { get; set; }
        public List<PropertyImport> InvalidRows { get; set; } = new();
    }

    public class EvaluationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public int UpdatedPropertiesCount { get; set; }
        public int TotalPropertiesCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Helper class for use types
    /// </summary>
    public static class PropertyUseTypes
    {
        public const string Residential = "RES";
        public const string Commercial = "COM";
        public const string Industrial = "IND";
        public const string Hospitality = "HOS";
        public const string Institutional = "INS";
        public const string PowerTransmission = "PWR";
        public const string Airport = "APT";

        public static readonly Dictionary<string, string> Descriptions = new()
        {
            { Residential, "Residential" },
            { Commercial, "Commercial" },
            { Industrial, "Industrial" },
            { Hospitality, "Hospitality" },
            { Institutional, "Institutional" },
            { PowerTransmission, "Power Transmission" },
            { Airport, "Properties Owned by Zambia Airport Corporation Limited" }
        };

        public static readonly Dictionary<string, decimal> ApprovedPoundageRates = new()
        {
            { Residential, 0.001m },
            { Commercial, 0.002m },
            { Industrial, 0.002m },
            { Hospitality, 0.002m },
            { Institutional, 0.002m },
            { PowerTransmission, 0.0015m },
            { Airport, 0.0015m }
        };

        public static List<string> GetAll() => Descriptions.Keys.ToList();

        public static List<PropertyType> GetDefaultPropertyTypes()
        {
            var types = new List<PropertyType>();
            int sortOrder = 1;

            foreach (var kvp in Descriptions)
            {
                types.Add(new PropertyType
                {
                    ShortName = kvp.Key,
                    Name = kvp.Value,
                    IsActive = true,
                    SortOrder = sortOrder++,
                    CreatedDate = DateTime.UtcNow
                });
            }

            return types;
        }

        public static List<PoundageRate> GetApprovedPoundageRates(List<PropertyType> propertyTypes, Guid? organisationId = null)
        {
            var rates = new List<PoundageRate>();
            var effectiveDate = DateTime.UtcNow.Date;

            foreach (var propertyType in propertyTypes)
            {
                if (ApprovedPoundageRates.TryGetValue(propertyType.ShortName, out decimal rate))
                {
                    rates.Add(new PoundageRate
                    {
                        PropertyTypeId = propertyType.Id,
                        OrganisationId = organisationId,
                        Rate = rate,
                        EffectiveFrom = effectiveDate,
                        IsCurrent = true,
                        CreatedDate = DateTime.UtcNow,
                        Notes = "Approved rate"
                    });
                }
            }

            return rates;
        }
    }

    /// <summary>
    /// Extension methods for currency formatting
    /// </summary>
    public static class CurrencyHelper
    {
        public static string ToZMW(this decimal amount)
        {
            return $"K {amount:N2}";
        }

        public static string ToZMW(this decimal? amount)
        {
            return amount.HasValue ? $"K {amount.Value:N2}" : "";
        }
    }
}