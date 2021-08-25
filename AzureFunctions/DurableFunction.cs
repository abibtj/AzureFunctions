using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public static class DurableFunction
    {
        // #2. Entry function calls orchestrator
        [FunctionName("OrchestratorFunction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            await context.CallActivityAsync("RequestApprovalFunction", null);
            using (var timeoutCts = new CancellationTokenSource())
            {
                DateTime dueTime = context.CurrentUtcDateTime.AddHours(72);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("ApprovalEvent");
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    outputs.Add(await context.CallActivityAsync<string>("ApprovalFunction", "Approved"));
                    //await context.CallActivityAsync("ProcessApproval", approvalEvent.Result);
                }
                else
                {
                    outputs.Add(await context.CallActivityAsync<string>("EscalationFunction", "Head of Department"));
                    //await context.CallActivityAsync("Escalate", null);
                }
            }

            return outputs;
        }

        //// #2. Entry function calls orchestrator
        //[FunctionName("OrchestratorFunction")]
        //public static async Task<List<string>> RunOrchestrator(
        //    [OrchestrationTrigger] IDurableOrchestrationContext context)
        //{
        //    var outputs = new List<string>();

        //    using var tokenSource = new CancellationTokenSource();

        //    //var deadline = context.CurrentUtcDateTime.AddSeconds(20000000);
        //    var deadline = DateTime.UtcNow.AddSeconds(5);
        //    var approvalEvent = context.WaitForExternalEvent("Approval");
        //    var timeoutEvent = context.CreateTimer(deadline, tokenSource.Token);

        //    var tasks = Task.WhenAny(approvalEvent, timeoutEvent);
        //    //var winner = Task.WaitAny(activityTask, timeoutTask);

        //    var winner = await tasks;

        //    if (winner == approvalEvent)
        //    {
        //        //tokenSource.Cancel(); // Cancel timeout event
        //        outputs.Add(await context.CallActivityAsync<string>("ApprovalFunction", "Approved"));
        //    }
        //    else
        //    {
        //        outputs.Add(await context.CallActivityAsync<string>("EscalationFunction", "Head of Department"));
        //    }

        //    if (!timeoutEvent.IsCompleted)
        //    {
        //        // All pending timers must be complete or canceled before the function exits.
        //        tokenSource.Cancel();
        //    }

        //    //outputs.Add(await context.CallActivityAsync<string>("ApprovalFunction", "Approved"));
        //    //outputs.Add(await context.CallActivityAsync<string>("ApprovalFunction", "Rejected"));
        //    //outputs.Add(await context.CallActivityAsync<string>("ApprovalFunction", "Escalated"));

        //    return outputs;
        //}


        // #3. Orchestrator calls activity function(s)
        [FunctionName("ApprovalFunction")]
        public static string RunApprovalFunction([ActivityTrigger] string action, ILogger log)
        {
            log.LogInformation($"Project proposal {action}.");
            return $"Your project proposal has been {action}!";
        }
       
        [FunctionName("RequestApprovalFunction")]
        public static string RunRequestApprovalFunction([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation("Approval Requested.");
            return "Approval Requested.";
        }

        // #4. Orchestrator calls activity function(s)
        [FunctionName("EscalationFunction")]
        public static string RunEscalationFunction([ActivityTrigger] string supervisor, ILogger log)
        {
            log.LogInformation($"Project proposal escalated to {supervisor}.");
            return $"Your project proposal has been escalated to {supervisor}.";
        }

        // #1. Entry function
        [FunctionName("HttpStartFunction")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}