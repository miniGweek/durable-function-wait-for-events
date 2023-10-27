using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Utility.cs;

namespace DeviceCommandIssuer
{
    public static class SimpleOrchestration
    {
        [FunctionName(nameof(IssueDeviceCommands_Orchestration))]
        public static async Task<List<string>> IssueDeviceCommands_Orchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            log = context.CreateReplaySafeLogger(log);

            var outputs = new List<string>();

            log.LogWarning("Beginning of Orchestration {name}", nameof(IssueDeviceCommands_Orchestration));


            await context.CallActivityAsync<string>(nameof(DoSomething_Activity), "Did activity 1 in Orchestration 2");
            await context.CallActivityAsync<string>(nameof(DoSomething_Activity), "Did activity 2 in Orchestration 2");

            var commandsToBeIssued = context.GetInput<CommandAndEvents>();

            if (commandsToBeIssued.Commands.Count > 0)
            {
                var waitForExternalEventList = new List<Task<string>>();
                foreach (var command in commandsToBeIssued.Commands)
                {
                    await context.CallActivityAsync(nameof(IssueCommand_Activity), command.CommandText);
                    waitForExternalEventList.Add(context.WaitForExternalEvent<string>(command.WaitForEventType, TimeSpan.FromSeconds(300)));
                }

                log.LogWarning("Waiting for all events to finish before proceeding.");
                await Task.WhenAll(waitForExternalEventList);

                foreach (var task in waitForExternalEventList)
                {
                    outputs.Add(task.Result);
                }
            }

            log.LogWarning("End of orchestration {orchestration}", nameof(DoSomething_Activity));
            return outputs;
        }

        [FunctionName(nameof(DoSomething_Activity))]
        public static async Task<string> DoSomething_Activity([ActivityTrigger] string name, ILogger log)
        {
            log.LogWarning("Saying hello to {name} in DoSomething_Activity.", name);
            return await Task.FromResult($"Hello {name}!");
        }

        [FunctionName(nameof(IssueCommand_Activity))]
        public static async Task<string> IssueCommand_Activity([ActivityTrigger] string commandText, ILogger log)
        {
            log.LogWarning("Issued command {command} from activity {activity}", commandText, nameof(IssueCommand_Activity));
            return await Task.FromResult($"Issued command {commandText}!");
        }

        [FunctionName(nameof(HttpStart_DeviceCommandOrchestration))]
        public static async Task<HttpResponseMessage> HttpStart_DeviceCommandOrchestration(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            var commandsToBeIssued =  req.GetRequestEntity<CommandAndEvents>();
            log.LogWarning("Before starting Orchestration {name} - with commands {commands}",
                nameof(HttpStart_DeviceCommandOrchestration),
                JsonConvert.SerializeObject(commandsToBeIssued));

            string instanceId = await starter.StartNewAsync(nameof(IssueDeviceCommands_Orchestration), commandsToBeIssued);

            log.LogWarning("Started 2nd Orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}