using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace LocalGov360.Services
{
    public interface IWorkflowFactory
    {
        Task<IWorkflow> CreateInstanceAsync(Guid workflowTemplateId, string initiatedBy, int ServiceId);
    }

    public class WorkflowFactory : IWorkflowFactory
    {
        private readonly ApplicationDbContext _db;
        public WorkflowFactory(ApplicationDbContext db) => _db = db;

        public async Task<IWorkflow> CreateInstanceAsync(Guid templateId, string initiatedBy, int ServiceId)
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
                    PaymentTemplateStep p => new PaymentInstanceStepAdapter(p, context.WorkflowId),
                    ApprovalTemplateStep a => new ApprovalInstanceStepAdapter(a, context.WorkflowId),
                    _ => throw new InvalidOperationException("Unknown step type")
                };
                workflow.AddStep(step);
            }

            // Persist instance
            var instance = new WorkflowInstance
            {
                Id = context.WorkflowId,
                Name = template.Name,
                InitiatedBy = initiatedBy,
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
                    _ => throw new InvalidOperationException()
                });

                persistStep.WorkflowInstanceId = instance.Id;
                _db.WorkflowInstanceSteps.Add(persistStep);
            }
            await _db.SaveChangesAsync();
            return workflow;
        }
    }

    internal class PaymentInstanceStepAdapter : WorkflowStepBase
    {
        public PaymentInstanceStepAdapter(PaymentTemplateStep cfg, Guid instanceId)
            : base(cfg.Name, cfg.Order, cfg.Description)
        {
            Type = StepType.Payment;
            Amount = cfg.Amount;
            Currency = cfg.Currency;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        public override Task<bool> ExecuteAsync(IWorkflowContext ctx)
        {
            // same logic as before
            StartedAt = DateTime.UtcNow;
            Status = StepStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ctx.Data[$"Payment_{Id}"] = new { Amount, Currency, TransactionId = Guid.NewGuid().ToString() };
            return Task.FromResult(true);
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
}
