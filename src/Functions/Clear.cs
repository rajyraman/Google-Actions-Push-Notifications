using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using PushNotificationsAPI.Actors;

namespace PushNotificationsAPI.Functions
{
    public static class Clear
    {
        [FunctionName("clear")]
        [OpenApiOperation(operationId: "clear", tags: "Subscriptions", Description = "Clears all existing subscriptions and stops any further push notifications", Summary = "Clears all existing subscriptions")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            log.LogInformation("Processing clear request.");
            var entityId = new EntityId(nameof(PushNotificationEntity), "GoogleAssistant");

            var stateResponse = await client.ReadEntityStateAsync<PushNotificationEntity>(entityId);

            if (!stateResponse.EntityExists)
            {
                return new NotFoundObjectResult("No subscriptions found");
            }

            await client.SignalEntityAsync<IPushNotifications>(entityId, proxy => proxy.Clear());

            return new OkResult();
        }
    }
}

