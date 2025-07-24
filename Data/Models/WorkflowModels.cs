using LocalGov360.Services;
using static LocalGov360.Data.Models.ServiceModels;

namespace LocalGov360.Data.Models
{
    public enum WorkflowStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
    public enum StepStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Approved,
        Rejected,
        SentBack
    }

    public enum StepType
    {
        Payment,
        Approval
    }

    // -------------  CONFIGURATION  -------------
    public class WorkflowTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? OrganisationId { get; set; }
        public virtual Organisation Organisation { get; set; }

        public ICollection<WorkflowTemplateStep> Steps { get; set; } = new List<WorkflowTemplateStep>();
    }

    public abstract class WorkflowTemplateStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid WorkflowTemplateId { get; set; }
        public WorkflowTemplate WorkflowTemplate { get; set; } = null!;
    }

    public class PaymentTemplateStep : WorkflowTemplateStep
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZMW";
    }

    public class ApprovalTemplateStep : WorkflowTemplateStep
    {
        public List<string> RequiredApprovers { get; set; } = new();
        public bool RequiresAll { get; set; }
        public int MinimumApprovals { get; set; } = 1;
    }

    // -------------  INSTANCE  -------------
    public class WorkflowInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string InitiatedBy { get; set; } = "";
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ServiceId { get; set; }
        public Guid WorkflowTemplateId { get; set; }
        public WorkflowTemplate Template { get; set; } = null!;
        public virtual Service Service { get; set; }
        public ICollection<WorkflowInstanceStep> Steps { get; set; } = new List<WorkflowInstanceStep>();
        public string ContextJson { get; set; } = "{}";   // IWorkflowContext serialized
    }

    public abstract class WorkflowInstanceStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public StepType Type { get; set; }
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid AssignedTo { get; set; }
        public string Comments { get; set; } = "";
        public Guid WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; } = null!;
    }

    public class PaymentInstanceStep : WorkflowInstanceStep
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZMW";
        public string? TransactionId { get; set; }
    }

    public class ApprovalInstanceStep : WorkflowInstanceStep
    {
        public List<string> RequiredApprovers { get; set; } = new();
        public List<string> ActualApprovers { get; set; } = new();
        public bool RequiresAll { get; set; }
        public int MinimumApprovals { get; set; }
    }

    // Core workflow interfaces
    public interface IWorkflowStep
    {
        Guid Id { get; }
        string Name { get; }
        StepType Type { get; }
        StepStatus Status { get; set; }
        int Order { get; }
        string Description { get; }
        DateTime? StartedAt { get; set; }
        DateTime? CompletedAt { get; set; }
        string AssignedTo { get; set; }
        string Comments { get; set; }

        Task<bool> ExecuteAsync(IWorkflowContext context);
        bool CanExecute(IWorkflowContext context);
        void Reset();
    }

    public interface IWorkflowContext
    {
        Guid WorkflowId { get; }
        Dictionary<string, object> Data { get; }
        string InitiatedBy { get; }
        DateTime CreatedAt { get; }
    }

    public interface IWorkflow
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        WorkflowStatus Status { get; set; }
        IList<IWorkflowStep> Steps { get; }
        IWorkflowContext Context { get; }

        Task<bool> StartAsync();
        Task<bool> ExecuteNextStepAsync();
        IWorkflowStep GetCurrentStep();
        void AddStep(IWorkflowStep step);
        void Reset();
    }

    // Workflow context implementation
    public class WorkflowContext : IWorkflowContext
    {
        public Guid WorkflowId { get; }
        public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public string InitiatedBy { get; }
        public DateTime CreatedAt { get; }

        public WorkflowContext(Guid workflowId, string initiatedBy)
        {
            WorkflowId = workflowId;
            InitiatedBy = initiatedBy;
            CreatedAt = DateTime.UtcNow;
        }
    }

    // Main workflow implementation
    public class Workflow : IWorkflow
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Description { get; private set; }
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
        public IList<IWorkflowStep> Steps { get; } = new List<IWorkflowStep>();
        public IWorkflowContext Context { get; private set; }

        public Workflow(string name, string description, string initiatedBy)
        {
            Name = name;
            Description = description;
            Context = new WorkflowContext(Id, initiatedBy);
        }

        public Workflow(string name, string description, string initiatedBy, int ServiceId)
        {
            Name = name;
            ServiceId = ServiceId;
            Description = description;
            Context = new WorkflowContext(Id, initiatedBy);
        }

        public async Task<bool> StartAsync()
        {
            if (Status != WorkflowStatus.Pending)
                return false;

            Status = WorkflowStatus.InProgress;
            return await ExecuteNextStepAsync();
        }

        public async Task<bool> ExecuteNextStepAsync()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null)
            {
                Status = WorkflowStatus.Completed;
                return true;
            }

            if (!currentStep.CanExecute(Context))
                return false;

            var result = await currentStep.ExecuteAsync(Context);

            if (!result)
            {
                Status = WorkflowStatus.Failed;
                return false;
            }

            // Check if workflow is complete
            if (Steps.All(s => s.Status == StepStatus.Completed || s.Status == StepStatus.Approved))
            {
                Status = WorkflowStatus.Completed;
            }

            return true;
        }

        public IWorkflowStep GetCurrentStep()
        {
            return Steps
                .Where(s => s.Status == StepStatus.Pending)
                .OrderBy(s => s.Order)
                .FirstOrDefault();
        }

        public void AddStep(IWorkflowStep step)
        {
            Steps.Add(step);
        }

        public void Reset()
        {
            Status = WorkflowStatus.Pending;
            foreach (var step in Steps)
            {
                step.Reset();
            }
        }
    }

    // Abstract base classes
    public abstract class WorkflowStepBase : IWorkflowStep
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; protected set; }
        public StepType Type { get; protected set; }
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public int Order { get; protected set; }
        public string Description { get; protected set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedTo { get; set; }
        public string Comments { get; set; }

        protected WorkflowStepBase(string name, int order, string description = "", string assignedTo = "")
        {
            Name = name;
            Order = order;
            Description = description;
            AssignedTo = assignedTo;
        }

        public abstract Task<bool> ExecuteAsync(IWorkflowContext context);

        public virtual bool CanExecute(IWorkflowContext context)
        {
            return Status == StepStatus.Pending;
        }

        public virtual void Reset()
        {
            Status = StepStatus.Pending;
            StartedAt = null;
            CompletedAt = null;
            Comments = "";
        }
    }
}
