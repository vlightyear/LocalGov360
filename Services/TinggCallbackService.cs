using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LocalGov360.Services
{
    public class TinggCallbackService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<TinggCallbackService> _logger;

        public TinggCallbackService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<TinggCallbackService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IResult> ReceiveCallbackAsync(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                string body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                _logger.LogInformation("Tingg callback received: {Payload}", body);
                var payload = JsonDocument.Parse(body).RootElement;

                var merchantTransactionId = payload.GetProperty("merchant_transaction_id").GetString();
                var requestStatusCode = payload.GetProperty("request_status_code").GetInt32();
                var requestStatusDescription = payload.GetProperty("request_status_description").GetString();

                await using var db = await _dbContextFactory.CreateDbContextAsync();

                var payment = await db.ServicePayments
                    .FirstOrDefaultAsync(p => p.TransactionReference == merchantTransactionId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for transactionReference: {Id}", merchantTransactionId);
                    return Results.NotFound($"Transaction {merchantTransactionId} not found");
                }

                payment.ResponsePayload = body;

                if (requestStatusCode == 178)
                {
                    payment.Status = "Paid";

                    // ✅ Mark corresponding workflow step as completed
                    var workflowStep = await db.WorkflowInstanceSteps
                        .FirstOrDefaultAsync(s => s.Id == payment.WorkflowStepId);

                    if (workflowStep != null)
                    {
                        workflowStep.Status = StepStatus.Completed;
                        workflowStep.CompletedAt = DateTime.UtcNow;

                        _logger.LogInformation("Workflow step {StepId} marked as completed", workflowStep.Id);

                        // ✅ Optional: check if this was the LAST step and mark the whole instance as completed
                        var allSteps = await db.WorkflowInstanceSteps
                            .Where(s => s.WorkflowInstanceId == workflowStep.WorkflowInstanceId)
                            .ToListAsync();

                        bool allDone = allSteps.All(s => s.Status == StepStatus.Completed);

                        if (allDone)
                        {
                            var instance = await db.WorkflowInstances
                                .FirstOrDefaultAsync(w => w.Id == workflowStep.WorkflowInstanceId);

                            if (instance != null)
                            {
                              
                                _logger.LogInformation("Workflow instance {InstanceId} marked as completed", instance.Id);
                            }
                        }
                    }
                }
                else
                {
                    payment.Status = requestStatusDescription ?? "Failed";
                }

                await db.SaveChangesAsync();

                return Results.Ok(new { message = "Callback processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Tingg payment callback.");
                return Results.StatusCode(500);
            }
        }
    }
}
