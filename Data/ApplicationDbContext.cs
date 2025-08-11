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
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<ServicePayment> ServicePayments { get; set; }

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
