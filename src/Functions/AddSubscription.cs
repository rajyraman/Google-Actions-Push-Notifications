using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using PushNotificationsAPI.Actors;

namespace PushNotificationsAPI.Functions
{
    public static class AddSubscription
    {
        [FunctionName("subscribe")]
        [OpenApiOperation(operationId: "subscribe", tags: "Subscriptions", Description = "Processes push notification subscription in the request body and adds it to list of subscribers", Summary = "Add a new push notification subscription")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(PushNotification), Description = "JSON request body containing { UserId and Location}")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "text/plain", bodyType: typeof(string), Description = "Accepted Response after subscription is accepted")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation("Processing subscribe request.");

            var entityId = new EntityId(nameof(PushNotificationEntity), "GoogleAssistant");

            var subscription = await req.Content.ReadAsAsync<PushNotification>();

            await client.SignalEntityAsync<IPushNotifications>(entityId, proxy => proxy.Subscribe(subscription));

            return new OkResult(); 
        }
    }
}

