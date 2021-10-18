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
using System.Collections.Generic;
using PushNotificationsAPI.Actors;

namespace PushNotificationsAPI.Functions
{
    public static class GetSubscriptions
    {
        [FunctionName("subscriptions")]
        [OpenApiOperation(operationId: "subscriptions", tags: "Subscriptions", Description = "Returns all existing subscriptions as a collection with UserId and Location", Summary = "Returns all existing subscriptions")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<PushNotification>), Description = "List of all push notification subscriptions")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            log.LogInformation("Processing subscriptions request.");
            var entityId = new EntityId(nameof(PushNotificationEntity), "GoogleAssistant");

            var stateResponse = await client.ReadEntityStateAsync<PushNotificationEntity>(entityId);
            if (stateResponse.EntityState != null)
            {
                var response = await stateResponse.EntityState.GetSubscriptions();
                return new OkObjectResult(response);
            }
            return new OkResult();
        }
    }
}
