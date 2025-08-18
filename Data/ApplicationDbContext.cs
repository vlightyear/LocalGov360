using LocalGov360.Data.Models;
using LocalGov360.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static LocalGov360.Data.Models.ServiceModels;

namespace LocalGov360.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
    {

        public DbSet<RevenueClass> RevenueClasses { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<ServicePayment> ServicePayments { get; set; }

        // Property-related DbSets
        public DbSet<ValuationRoll> CouncilValuationRolls { get; set; }
        public DbSet<PropertyType> CouncilPropertyTypes { get; set; }
        public DbSet<PoundageRate> CouncilPoundageRates { get; set; }
        public DbSet<Property> CouncilProperties { get; set; }
        public DbSet<PropertyEvaluation> CouncilPropertyEvaluations { get; set; }





        public DbSet<ServiceInvoice> ServiceInvoices { get; set; }
        public DbSet<ServiceInvoiceLineItem> ServiceInvoiceLineItems { get; set; }

        public DbSet<TinggConfiguration> TinggConfigurations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceField> ServiceFields { get; set; }
        public DbSet<ServiceSubmission> ServiceSubmissions { get; set; }
        public DbSet<ServiceSubmissionValue> ServiceSubmissionValues { get; set; }
        public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; }
        public DbSet<WorkflowTemplateStep> WorkflowTemplateSteps { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

            // Configure other Identity entities if needed
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // ServiceField configuration
            modelBuilder.Entity<ServiceField>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FieldType).HasConversion<string>();

                entity.HasOne(e => e.Service)
                    .WithMany(f => f.Fields)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ServiceId, e.Name }).IsUnique();
            });

            // ServiceSubmission configuration
            modelBuilder.Entity<ServiceSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SubmittedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Service)
                    .WithMany(f => f.Submissions)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceSubmissionValue configuration
            modelBuilder.Entity<ServiceSubmissionValue>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Submission)
                    .WithMany(s => s.Values)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Field)
                    .WithMany(f => f.SubmissionValues)
                    .HasForeignKey(e => e.FieldId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.SubmissionId, e.FieldId }).IsUnique();
            });
            // Entity Framework Configuration in OnModelCreating method

            ///ValuationRoll




            modelBuilder.Entity<ValuationRoll>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Council).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ValuationDate).IsRequired();
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.RollNumber).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.Council, e.Year });
                entity.HasIndex(e => e.RollNumber);
            });

            // Configure PropertyType
            modelBuilder.Entity<PropertyType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.ShortName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure PoundageRate
            modelBuilder.Entity<PoundageRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rate).HasColumnType("decimal(10,6)").IsRequired();
                entity.Property(e => e.EffectiveFrom).IsRequired();
                entity.Property(e => e.IsCurrent).HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).HasMaxLength(200);
                entity.Property(e => e.ModifiedBy).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.PropertyType)
                      .WithMany(pt => pt.PoundageRates)
                      .HasForeignKey(e => e.PropertyTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ValuationRoll)
                      .WithMany(vr => vr.PoundageRates)
                      .HasForeignKey(e => e.ValuationRollId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.PropertyTypeId, e.EffectiveFrom });
                entity.HasIndex(e => e.IsCurrent);
            });

            // Configure Property
            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PropertyNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.StreetAddress).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Leaseholder).HasMaxLength(200);
                entity.Property(e => e.Use).IsRequired().HasMaxLength(10);
                entity.Property(e => e.LandExtHa).HasColumnType("decimal(10,4)");
                entity.Property(e => e.LandValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ValueOfImprovements).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalRateableValue).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.Poundage).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PoundageRate).HasColumnType("decimal(10,6)");
                entity.Property(e => e.EvaluationAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.ValuationRoll)
                      .WithMany(vr => vr.Properties)
                      .HasForeignKey(e => e.ValuationRollId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PropertyType)
                      .WithMany(pt => pt.Properties)
                      .HasForeignKey(e => e.PropertyTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ValuationRollId, e.PropertyNumber }).IsUnique();
                entity.HasIndex(e => e.PropertyNumber);
                entity.HasIndex(e => e.Use);
                entity.HasIndex(e => e.TotalRateableValue);
            });

            // Configure PropertyEvaluation
            modelBuilder.Entity<PropertyEvaluation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.Period).IsRequired();
                entity.Property(e => e.RateableValue).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PoundageRate).HasColumnType("decimal(10,6)").IsRequired();
                entity.Property(e => e.EvaluationAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsPaid).HasDefaultValue(false);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(200);
                entity.Property(e => e.ModifiedBy).HasMaxLength(200);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                      .WithMany(p => p.PropertyEvaluations)
                      .HasForeignKey(e => e.PropertyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ValuationRoll)
                      .WithMany(vr => vr.PropertyEvaluations)
                      .HasForeignKey(e => e.ValuationRollId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PropertyId, e.Year, e.Period }).IsUnique();
                entity.HasIndex(e => new { e.Year, e.Period });
                entity.HasIndex(e => e.InvoiceNumber);
                entity.HasIndex(e => e.IsPaid);
            });

            // Seed default property types
            SeedPropertyTypes(modelBuilder);
        }

        private void SeedPropertyTypes(ModelBuilder modelBuilder)
        {
            // Use fixed dates for seeding to avoid migration issues
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var propertyTypes = new[]
            {
                new PropertyType { Id = 1, ShortName = "RES", Name = "Residential", IsActive = true, SortOrder = 1, CreatedDate = seedDate },
                new PropertyType { Id = 2, ShortName = "COM", Name = "Commercial", IsActive = true, SortOrder = 2, CreatedDate = seedDate },
                new PropertyType { Id = 3, ShortName = "IND", Name = "Industrial", IsActive = true, SortOrder = 3, CreatedDate = seedDate },
                new PropertyType { Id = 4, ShortName = "HOS", Name = "Hospitality", IsActive = true, SortOrder = 4, CreatedDate = seedDate },
                new PropertyType { Id = 5, ShortName = "INS", Name = "Institutional", IsActive = true, SortOrder = 5, CreatedDate = seedDate },
                new PropertyType { Id = 6, ShortName = "PWR", Name = "Power Transmission", IsActive = true, SortOrder = 6, CreatedDate = seedDate },
                new PropertyType { Id = 7, ShortName = "APT", Name = "Properties Owned by Zambia Airport Corporation Limited", IsActive = true, SortOrder = 7, CreatedDate = seedDate }
            };

            modelBuilder.Entity<PropertyType>().HasData(propertyTypes);

            // Seed default poundage rates
            var poundageRates = new[]
            {
                new PoundageRate { Id = 1, PropertyTypeId = 1, Rate = 0.001m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Residential" },
                new PoundageRate { Id = 2, PropertyTypeId = 2, Rate = 0.002m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Commercial" },
                new PoundageRate { Id = 3, PropertyTypeId = 3, Rate = 0.002m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Industrial" },
                new PoundageRate { Id = 4, PropertyTypeId = 4, Rate = 0.002m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Hospitality" },
                new PoundageRate { Id = 5, PropertyTypeId = 5, Rate = 0.002m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Institutional" },
                new PoundageRate { Id = 6, PropertyTypeId = 6, Rate = 0.0015m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Power Transmission" },
                new PoundageRate { Id = 7, PropertyTypeId = 7, Rate = 0.0015m, EffectiveFrom = seedDate, IsCurrent = true, CreatedDate = seedDate, Notes = "Approved rate for Airport" }
            };

            modelBuilder.Entity<PoundageRate>().HasData(poundageRates);
        
    







// Configure Workflow Template relationships
modelBuilder.Entity<WorkflowTemplate>()
                .HasMany(w => w.Steps)
                .WithOne(s => s.WorkflowTemplate)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Workflow Template Step inheritance
            modelBuilder.Entity<WorkflowTemplateStep>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<PaymentTemplateStep>(nameof(PaymentTemplateStep))
                .HasValue<ApprovalTemplateStep>(nameof(ApprovalTemplateStep))
                .HasValue<InspectionTemplateStep>(nameof(InspectionTemplateStep));

            // Configure Workflow Instance relationships
            modelBuilder.Entity<WorkflowInstance>()
                .HasMany(w => w.Steps)
                .WithOne(s => s.WorkflowInstance)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Workflow Instance Step inheritance
            modelBuilder.Entity<WorkflowInstanceStep>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<PaymentInstanceStep>(nameof(PaymentInstanceStep))
                .HasValue<ApprovalInstanceStep>(nameof(ApprovalInstanceStep))
                .HasValue<InspectionInstanceStep>(nameof(InspectionInstanceStep));

            // Configure JSON conversion for List<string> properties in Template Steps
            // Map to existing columns without creating new ones
            modelBuilder.Entity<ApprovalTemplateStep>()
                .Property(e => e.RequiredApprovers)
                .HasColumnName("RequiredApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<ApprovalTemplateStep>()
                .Property(e => e.RequiresAll)
                .HasColumnName("RequiresAll");

            modelBuilder.Entity<ApprovalTemplateStep>()
                .Property(e => e.MinimumApprovals)
                .HasColumnName("MinimumApprovals");

            modelBuilder.Entity<InspectionTemplateStep>()
                .Property(e => e.RequiredApprovers)
                .HasColumnName("RequiredApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<InspectionTemplateStep>()
                .Property(e => e.RequiresAll)
                .HasColumnName("RequiresAll");

            modelBuilder.Entity<InspectionTemplateStep>()
                .Property(e => e.MinimumApprovals)
                .HasColumnName("MinimumApprovals");

            // Configure JSON conversion for List<string> properties in Instance Steps
            // Map to existing columns without creating new ones
            modelBuilder.Entity<ApprovalInstanceStep>()
                .Property(e => e.RequiredApprovers)
                .HasColumnName("RequiredApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<ApprovalInstanceStep>()
                .Property(e => e.ActualApprovers)
                .HasColumnName("ActualApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<ApprovalInstanceStep>()
                .Property(e => e.RequiresAll)
                .HasColumnName("RequiresAll");

            modelBuilder.Entity<ApprovalInstanceStep>()
                .Property(e => e.MinimumApprovals)
                .HasColumnName("MinimumApprovals");

            modelBuilder.Entity<InspectionInstanceStep>()
                .Property(e => e.RequiredApprovers)
                .HasColumnName("RequiredApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<InspectionInstanceStep>()
                .Property(e => e.ActualApprovers)
                .HasColumnName("ActualApprovers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<InspectionInstanceStep>()
                .Property(e => e.RequiresAll)
                .HasColumnName("RequiresAll");

            modelBuilder.Entity<InspectionInstanceStep>()
                .Property(e => e.MinimumApprovals)
                .HasColumnName("MinimumApprovals");

            // Configure decimal precision for payment amounts
            modelBuilder.Entity<PaymentTemplateStep>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentInstanceStep>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            // Configure foreign key relationships
            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(w => w.Template)
                .WithMany()
                .HasForeignKey(w => w.WorkflowTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(w => w.InitiatedBy)
                .WithMany()
                .HasForeignKey(w => w.InitiatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(w => w.Service)
                .WithMany()
                .HasForeignKey(w => w.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowTemplate>()
                .HasOne(w => w.Organisation)
                .WithMany()
                .HasForeignKey(w => w.OrganisationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes for better performance
            modelBuilder.Entity<WorkflowInstance>()
                .HasIndex(w => w.Status);

            modelBuilder.Entity<WorkflowInstance>()
                .HasIndex(w => w.InitiatedById);

            modelBuilder.Entity<WorkflowInstance>()
                .HasIndex(w => w.ServiceId);

            modelBuilder.Entity<WorkflowInstanceStep>()
                .HasIndex(w => w.Status);

            modelBuilder.Entity<WorkflowInstanceStep>()
                .HasIndex(w => w.Order);

            modelBuilder.Entity<WorkflowTemplateStep>()
                .HasIndex(w => w.Order);

            // Configure string lengths to avoid nvarchar(max) where appropriate
            modelBuilder.Entity<WorkflowTemplate>()
                .Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<WorkflowTemplate>()
                .Property(e => e.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<WorkflowTemplateStep>()
                .Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<WorkflowTemplateStep>()
                .Property(e => e.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<WorkflowInstance>()
                .Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<WorkflowInstanceStep>()
                .Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<PaymentTemplateStep>()
                .Property(e => e.Currency)
                .HasMaxLength(3)
                .IsRequired();

            modelBuilder.Entity<PaymentInstanceStep>()
                .Property(e => e.Currency)
                .HasMaxLength(3)
                .IsRequired();

            modelBuilder.Entity<InspectionInstanceStep>()
                .Property(e => e.InspectionFile)
                .HasMaxLength(500);

        }
    }
}
