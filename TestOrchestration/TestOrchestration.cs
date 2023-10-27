using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Models;

namespace TestRunner
{
    public static class TestOrchestration
    {
        [FunctionName(nameof(RootOrchestration_Orchestration))]
        public static async Task<List<string>> RootOrchestration_Orchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var outputs = new List<string>();

            log = context.CreateReplaySafeLogger(log);

            var commandsPayload = new CommandAndEvents()
            {
                Commands = new List<CommandAndEvent>()
                {
                    new() { CommandText = "Command1", WaitForEventType = "Event1" },
                    new() { CommandText = "Command2", WaitForEventType = "Event2" }
                }
            };

            await context.CallActivityAsync<string>(nameof(TestOrchestration_Activity1_Activity), "Did activity 1");
            await context.CallActivityAsync<string>(nameof(TestOrchestration_Activity1_Activity), "Did activity 1 again");

            log.LogWarning("Before waiting for the external orchestration Scenario 1, in {name}", nameof(TestOrchestration_Activity1_Activity));

            outputs.AddRange(await context.CallSubOrchestratorAsync<IEnumerable<string>>(
                nameof(WaitForExternalOrchestration.WaitForExternalOrchestration_Orchestration), commandsPayload));

            log.LogWarning("Finished execution of all commands - Scenario 1, and events received.");

            var commandsPayload2 = new CommandAndEvents()
            {
                Commands = new List<CommandAndEvent>()
                {
                    new() { CommandText = "Command3", WaitForEventType = "Event3" }
                }
            };
            var commandsPayload3 = new CommandAndEvents()
            {
                Commands = new List<CommandAndEvent>()
                {
                    new() { CommandText = "Command4", WaitForEventType = "Event4" }
                }
            };

            var commandOrchestration1 = context.CallSubOrchestratorAsync<IEnumerable<string>>(
                nameof(WaitForExternalOrchestration.WaitForExternalOrchestration_Orchestration), commandsPayload2);

            var commandOrchestration2 = context.CallSubOrchestratorAsync<IEnumerable<string>>(
                nameof(WaitForExternalOrchestration.WaitForExternalOrchestration_Orchestration), commandsPayload3);

            log.LogWarning("Before waiting for the external orchestration Scenario 2, in {name}", nameof(TestOrchestration_Activity1_Activity));
            await Task.WhenAll(commandOrchestration1, commandOrchestration2);

            outputs.AddRange(commandOrchestration1.Result);
            outputs.AddRange(commandOrchestration2.Result);

            log.LogWarning("Finished execution of all commands - Scenario 2, and events received.");

            log.LogWarning("End of Orchestration {name}", nameof(RootOrchestration_Orchestration));
            return outputs;
        }

        [FunctionName(nameof(TestOrchestration_Activity1_Activity))]
        public static string TestOrchestration_Activity1_Activity([ActivityTrigger] string message, ILogger log)
        {
            log.LogWarning("{message}. In activity {activity}", message, nameof(TestOrchestration_Activity1_Activity));
            return $"Executed {message}!";
        }

        [FunctionName(nameof(HttpStart_TestOrchestration))]
        public static async Task<HttpResponseMessage> HttpStart_TestOrchestration(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            string instanceId = await starter.StartNewAsync(nameof(RootOrchestration_Orchestration), null);

            log.LogWarning("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}