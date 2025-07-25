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

            //Workflow
            modelBuilder.Entity<WorkflowTemplate>()
            .HasMany(w => w.Steps)
            .WithOne(s => s.WorkflowTemplate)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowTemplateStep>()
                        .HasDiscriminator<string>("Discriminator")
                        .HasValue<PaymentTemplateStep>(nameof(PaymentTemplateStep))
                        .HasValue<ApprovalTemplateStep>(nameof(ApprovalTemplateStep));

            modelBuilder.Entity<WorkflowInstance>()
                        .HasMany(w => w.Steps)
                        .WithOne(s => s.WorkflowInstance)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowInstanceStep>()
                        .HasDiscriminator<string>("Discriminator")
                        .HasValue<PaymentInstanceStep>(nameof(PaymentInstanceStep))
                        .HasValue<ApprovalInstanceStep>(nameof(ApprovalInstanceStep));
        }
    }
}
