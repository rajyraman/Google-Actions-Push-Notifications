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
    public static class RemoveSubscription
    {
        [FunctionName("unsubscribe")]
        [OpenApiOperation(operationId: "unsubscribe", tags: "Subscriptions", Description = "Processes push notification unsubscribe request in URL path and unsubscribes the user from further push notifications", Summary = "Removes an existing push notification subscription")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "text/plain", bodyType: typeof(string), Description = "Accepted Response after subscription is accepted for removal")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "user/{userId}")] HttpRequestMessage req,
            string userId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation("Processing unsubscribe request.");

            var entityId = new EntityId(nameof(PushNotificationEntity), "GoogleAssistant");

            await client.SignalEntityAsync<IPushNotifications>(entityId, proxy => proxy.UnSubscribe(userId));

            return new OkResult();
        }
    }
}