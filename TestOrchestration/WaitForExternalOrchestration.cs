using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Models;
using Utility.cs;
using OrchestrationRuntimeStatus = Models.OrchestrationRuntimeStatus;

namespace TestRunner
{
    internal class WaitForExternalOrchestration
    {
        private static readonly HttpClient _httpClient = new();

        [FunctionName(nameof(WaitForExternalOrchestration_Orchestration))]
        public static async Task<IEnumerable<string>> WaitForExternalOrchestration_Orchestration([OrchestrationTrigger] IDurableOrchestrationContext executionContext, ILogger log)
        {
            log = executionContext.CreateReplaySafeLogger(log);

            log.LogWarning("In orchestration {orchestrationName} about to call activity external activity: {activity}",
                nameof(WaitForExternalOrchestration_Orchestration),
                nameof(TriggerExternalOrchestration_Activity));

            var commandsToBeIssued = executionContext.GetInput<CommandAndEvents>();

            var orchestrationInitiationResponse =
                await executionContext.CallActivityAsync<OrchestrationInitiationResponse>(nameof(TriggerExternalOrchestration_Activity), commandsToBeIssued);

            var retryOption = new RetryOptions(new TimeSpan(0, 0, 0, 3), 10)
            {
                BackoffCoefficient = 2,
                MaxRetryInterval = new TimeSpan(0, 0, 0, 10),
                RetryTimeout = new TimeSpan(0, 0, 10, 0),
            };


            log.LogWarning("Start waiting for Orchestration 02");

            var eventData = await executionContext.CallActivityWithRetryAsync<IEnumerable<string>>(
                nameof(CheckForOrchestrationCompletion_Activity), retryOption, Tuple.Create("WaitFor_Orchestration02", orchestrationInitiationResponse));

            log.LogWarning("Finished waiting for Orchestration 02");
            return eventData;
        }

        [FunctionName(nameof(TriggerExternalOrchestration_Activity))]
        public static async Task<OrchestrationInitiationResponse> TriggerExternalOrchestration_Activity([ActivityTrigger] IDurableActivityContext executionContext, ILogger log)
        {
            log.LogWarning("In activity {activity} about to trigger external orchestration", nameof(TriggerExternalOrchestration_Activity));

            var commandsToBeIssued = executionContext.GetInput<CommandAndEvents>();

            var deviceCommandOrchestrationEndpoint =
                new Uri("ExternalOrchestrationEndpoint".GetThisEnvironmentVariable());

            var orchestrationStartResponse = await _httpClient.PostAsJsonAsync(deviceCommandOrchestrationEndpoint, commandsToBeIssued);
            var orchestrationInitiatedResponseString = await orchestrationStartResponse.Content.ReadAsStringAsync();

            return JsonHelper.DeserializeObject<OrchestrationInitiationResponse>(orchestrationInitiatedResponseString);
        }

        [FunctionName(nameof(CheckForOrchestrationCompletion_Activity))]
        public static async Task<IEnumerable<string>> CheckForOrchestrationCompletion_Activity([ActivityTrigger] IDurableActivityContext executionContext, ILogger log)
        {
            log.LogWarning("In activity {activity} about to check status of orchestration", nameof(CheckForOrchestrationCompletion_Activity));
            var orchestrationResponse = executionContext.GetInput<Tuple<string, OrchestrationInitiationResponse>>();

            var orchestrationStatusResponseMessage = await _httpClient.GetAsync(orchestrationResponse.Item2.StatusQueryGetUri);
            var orchestrationStatusObject = JsonHelper.DeserializeObject<OrchestrationStatusResponse<IEnumerable<string>>>(await orchestrationStatusResponseMessage.Content.ReadAsStringAsync());

            log.LogWarning("Status of orchestration is {status}", orchestrationStatusObject.RuntimeStatus);

            if (orchestrationStatusObject.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                log.LogWarning("Operation done");
                return orchestrationStatusObject.Output;
            }

            log.LogWarning("Operation not done yet.");
            throw new Exception("Operation not done yet.");
        }
    }
}
