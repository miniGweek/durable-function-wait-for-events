using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace TestRunner
{
    public static class ResumeOrchestration
    {
        [FunctionName(nameof(ResumeHttpTrigger))]
        public static async Task<IActionResult> ResumeHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogWarning("In function {function}", nameof(ResumeHttpTrigger));

            string instanceId = req.Query["instanceId"];
            string eventName = req.Query["eventName"];
            string eventText = req.Query["eventText"];

            log.LogWarning("In function {function}, raising event {eventName} = {eventText} for instanceId {instanceId}", nameof(ResumeHttpTrigger), eventName, eventText, instanceId);
            await client.RaiseEventAsync(instanceId, eventName, eventText);

            string responseMessage = string.IsNullOrEmpty(instanceId)
                ? "This HTTP triggered function executed successfully. Pass an instanceId in the query string or in the request body for a personalized response."
                : $"Resuming orchestration with instanceId {instanceId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
