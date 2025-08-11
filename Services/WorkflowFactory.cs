using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LocalGov360.Services
{
    public interface IWorkflowFactory
    {
        Task<WorkflowInstance> CreateInstanceAsync(Guid workflowTemplateId, string initiatedBy, int ServiceId);
    }

    public class WorkflowFactory : IWorkflowFactory
    {
        private readonly ApplicationDbContext _db;
        private readonly TinggPaymentService _paymentService;

        public WorkflowFactory(ApplicationDbContext db, TinggPaymentService paymentService)
        {
            _db = db;
            _paymentService = paymentService;
        }

        public async Task<WorkflowInstance> CreateInstanceAsync(Guid templateId, string initiatedBy, int ServiceId)
        {
            var template = await _db.WorkflowTemplates
                                    .Include(t => t.Steps.OrderBy(s => s.Order))
                                    .SingleAsync(t => t.Id == templateId);

            var context = new WorkflowContext(Guid.NewGuid(), initiatedBy);
            var workflow = new Workflow(template.Name, template.Description, initiatedBy, ServiceId);

            foreach (var ts in template.Steps)
            {
                IWorkflowStep step = ts switch
                {
                    PaymentTemplateStep p => new PaymentInstanceStepAdapter(p, context.WorkflowId, _paymentService),
                    ApprovalTemplateStep a => new ApprovalInstanceStepAdapter(a, context.WorkflowId),
                    InspectionTemplateStep i => new InspectionInstanceStepAdapter(i, context.WorkflowId),
                    _ => throw new InvalidOperationException("Unknown step type")
                };

                if (step.Order == 1)
                {
                    step.Status = StepStatus.InProgress;
                    step.StartedAt = DateTime.UtcNow;
                }
                workflow.AddStep(step);
            }

            var instance = new WorkflowInstance
            {
                Id = context.WorkflowId,
                Name = template.Name,
                InitiatedById = initiatedBy,
                ServiceId = ServiceId,
                WorkflowTemplateId = templateId,
                ContextJson = JsonSerializer.Serialize(context.Data)
            };
            _db.WorkflowInstances.Add(instance);

            foreach (var s in workflow.Steps)
            {
                var persistStep = (WorkflowInstanceStep)(s switch
                {
                    PaymentInstanceStepAdapter p => new PaymentInstanceStep
                    {
                        Id = p.Id,
                        Order = p.Order,
                        Name = p.Name,
                        Type = StepType.Payment,
                        Amount = p.Amount,
                        Currency = p.Currency
                    },
                    ApprovalInstanceStepAdapter a => new ApprovalInstanceStep
                    {
                        Id = a.Id,
                        Order = a.Order,
                        Name = a.Name,
                        Type = StepType.Approval,
                        RequiredApprovers = a.RequiredApprovers,
                        RequiresAll = a.RequiresAll,
                        MinimumApprovals = a.MinimumApprovals
                    },
                    InspectionInstanceStepAdapter i => new InspectionInstanceStep
                    {
                        Id = i.Id,
                        Order = i.Order,
                        Name = i.Name,
                        Type = StepType.Inspection,
                        RequiredApprovers = i.RequiredApprovers,
                        RequiresAll = i.RequiresAll,
                        MinimumApprovals = i.MinimumApprovals
                    },
                    _ => throw new InvalidOperationException("Unknown step type")
                });

                persistStep.WorkflowInstanceId = instance.Id;

                if (persistStep.Order == 1)
                {
                    persistStep.Status = StepStatus.InProgress;
                    persistStep.StartedAt = DateTime.UtcNow;
                }
                _db.WorkflowInstanceSteps.Add(persistStep);
            }
            await _db.SaveChangesAsync();
            return instance;
        }
    }

    internal class PaymentInstanceStepAdapter : WorkflowStepBase
    {
        private readonly TinggPaymentService _paymentService;
        public Guid WorkflowInstanceId { get; }

        public PaymentInstanceStepAdapter(PaymentTemplateStep cfg, Guid instanceId, TinggPaymentService paymentService)
            : base(cfg.Name, cfg.Order, cfg.Description)
        {
            Type = StepType.Payment;
            Amount = cfg.Amount;
            Currency = cfg.Currency;
            _paymentService = paymentService;
            WorkflowInstanceId = instanceId;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        public override async Task<bool> ExecuteAsync(IWorkflowContext ctx)
        {
            StartedAt = DateTime.UtcNow;
            Status = StepStatus.InProgress;

            try
            {
                var organisationId = Guid.Parse(ctx.Data["OrganisationId"].ToString());
                var serviceId = int.Parse(ctx.Data["ServiceId"].ToString()); // ✅ Parses as int
                var userId = ctx.InitiatedBy;

                var payment = await _paymentService.InitiateTinggPaymentAsync(
                    organisationId,
                    WorkflowInstanceId,
                    Id,
                    serviceId,
                    Amount,
                    userId);

                ctx.Data[$"Payment_{Id}"] = new
                {
                    payment.Id,
                    payment.TransactionReference,
                    payment.PaymentUrl,
                    payment.Status
                };

                Status = StepStatus.Completed;
                CompletedAt = DateTime.UtcNow;

                return true;
            }
            catch (Exception ex)
            {
                Status = StepStatus.Failed;
                ctx.Data[$"Payment_{Id}_Error"] = ex.Message;
                throw;
            }
        }
    }

    internal class ApprovalInstanceStepAdapter : WorkflowStepBase
    {
        public ApprovalInstanceStepAdapter(ApprovalTemplateStep cfg, Guid instanceId)
            : base(cfg.Name, cfg.Order, cfg.Description)
        {
            Type = StepType.Approval;
            RequiredApprovers = cfg.RequiredApprovers;
            RequiresAll = cfg.RequiresAll;
            MinimumApprovals = cfg.MinimumApprovals;
        }

        public List<string> RequiredApprovers { get; }
        public List<string> ActualApprovers { get; } = new();
        public bool RequiresAll { get; }
        public int MinimumApprovals { get; }

        public override Task<bool> ExecuteAsync(IWorkflowContext ctx) => Task.FromResult(true);
    }

    internal class InspectionInstanceStepAdapter : WorkflowStepBase
    {
        public InspectionInstanceStepAdapter(InspectionTemplateStep cfg, Guid instanceId)
            : base(cfg.Name, cfg.Order, cfg.Description)
        {
            Type = StepType.Inspection;
            RequiredApprovers = cfg.RequiredApprovers;
            RequiresAll = cfg.RequiresAll;
            MinimumApprovals = cfg.MinimumApprovals;
        }

        public List<string> RequiredApprovers { get; }
        public List<string> ActualApprovers { get; } = new();
        public bool RequiresAll { get; }
        public int MinimumApprovals { get; }

        public override async Task<bool> ExecuteAsync(IWorkflowContext ctx)
        {
            StartedAt = DateTime.UtcNow;
            Status = StepStatus.InProgress;

            try
            {
                ctx.Data[$"Inspection_{Id}"] = new
                {
                    Status = "Awaiting Inspection",
                    StartedAt = StartedAt,
                    RequiredApprovers = RequiredApprovers,
                    RequiresAll = RequiresAll,
                    MinimumApprovals = MinimumApprovals
                };

                return true;
            }
            catch (Exception ex)
            {
                Status = StepStatus.Failed;
                ctx.Data[$"Inspection_{Id}_Error"] = ex.Message;
                throw;
            }
        }
    }
}