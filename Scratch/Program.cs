// See https://aka.ms/new-console-template for more information

using Models;
using Newtonsoft.Json;
using Scratch;
using Utility.cs;

Console.WriteLine("Hello, World!");
HttpClient client = new HttpClient();


var testOrchestrationEndpoint = "http://localhost:7193/api/HttpStart_TestOrchestration";
var deviceCommandResumeEndpoint = "http://localhost:7129/api/ResumeHttpTrigger";

await Task.Delay(TimeSpan.FromSeconds(10));
var testOrchestrationResponse = await client.GetAsync(new Uri(testOrchestrationEndpoint));
var testOrchestrationResponseEntity = JsonHelper.DeserializeObject<OrchestrationInitiationResponse>(await testOrchestrationResponse.Content.ReadAsStringAsync());

var event1 = Helper.GetUserInput();
_ = await client.GetAsync(new Uri($"{deviceCommandResumeEndpoint}?instanceId={event1.Item1}&eventName={event1.Item2}&eventText={event1.Item3}"));

var event2 = Helper.GetUserInput();
_ = await client.GetAsync(new Uri($"{deviceCommandResumeEndpoint}?instanceId={event2.Item1}&eventName={event2.Item2}&eventText={event2.Item3}"));

var event3 = Helper.GetUserInput();
_ = await client.GetAsync(new Uri($"{deviceCommandResumeEndpoint}?instanceId={event3.Item1}&eventName={event3.Item2}&eventText={event3.Item3}"));

var event4 = Helper.GetUserInput();
_ = await client.GetAsync(new Uri($"{deviceCommandResumeEndpoint}?instanceId={event4.Item1}&eventName={event4.Item2}&eventText={event4.Item3}"));

Console.WriteLine("Waiting for orchestrations to finish. Hit enter when the Orchestration has finished.");
Console.ReadLine();

Console.Write($"Orchestration finished. Getting outputs");
var finishedResponseMessage = await client.GetAsync(testOrchestrationResponseEntity!.StatusQueryGetUri);
var finishedResponseEntity =
    JsonHelper.DeserializeObject<OrchestrationStatusResponse<IEnumerable<string>>>(await finishedResponseMessage.Content
        .ReadAsStringAsync());
Console.Write($"Orchestration finished. Outputs are: {JsonConvert.SerializeObject(finishedResponseEntity!.Output)}");
Console.ReadLine();

