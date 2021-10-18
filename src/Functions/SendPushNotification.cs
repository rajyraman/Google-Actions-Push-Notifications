using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PushNotificationsAPI.Functions
{
    public class SendPushNotification
    {
		private readonly HttpClient _client;

		public SendPushNotification(IHttpClientFactory httpClientFactory)
		{
			this._client = httpClientFactory.CreateClient();
		}

		[FunctionName("sendpushnotification")]
		[OpenApiOperation(operationId: "sendpushnotification", tags: "Subscriptions", Description = "Send Push Notification to User based on UserId", Summary = "Send Push Notification to User")]
		[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		[OpenApiRequestBody("application/json", typeof(Notification), Description = "JSON request body containing { UserId, Notification Title and Text}")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "OK response, when notification has been sent to user")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
			try
			{
				log.LogInformation("Processing sendpushnotification request.");

				var initializer = new ServiceAccountCredential.Initializer(Environment.GetEnvironmentVariable("SERVICE_ACCOUNT_EMAIL"))
				{
					UseJwtAccessWithScopes = true,
					Scopes = new[] { "https://www.googleapis.com/auth/actions.fulfillment.conversation" },
				}.FromPrivateKey($"-----BEGIN PRIVATE KEY-----\n{Environment.GetEnvironmentVariable("SERVICE_ACCOUNT_KEY")}\n-----END PRIVATE KEY-----\n");
				var cred = new ServiceAccountCredential(initializer);
				var token = await cred.GetAccessTokenForRequestAsync("https://accounts.google.com/o/oauth2/auth");
				_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


				var notification = await req.Content.ReadAsAsync<Notification>();
				var content = new PushNotificationRequest
				{
					CustomPushMessage = new Message
					{
						UserNotification = new NotificationContent
						{
							Title = notification.Title
						},
						Target = new MessageTarget
						{
							UserId = notification.UserId,
							Intent = "IncomingPushNotifications_Intent",
							Locale = "en-AU",
							Argument = notification.Argument
						}
					},
					IsInSandbox = true
				};
				var response = await _client.PostAsync("https://actions.googleapis.com/v2/conversations:send", 
					new StringContent(JsonSerializer.Serialize(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase}), Encoding.UTF8, "application/json"));
				response.EnsureSuccessStatusCode();
				return new OkResult();
			}
			catch(Exception ex)
            {
				log.LogError(ex.Message, ex);
				throw;
			}
        }
    }

	public class Notification
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public ActionsArgument Argument { get; set; }
    }
	public class PushNotificationRequest
	{
		public Message CustomPushMessage { get; set; }
		public bool IsInSandbox { get; set; }
	}

	public class Message
	{
		public NotificationContent UserNotification { get; set; }
		public MessageTarget Target { get; set; }
    }
	public class NotificationContent
	{
		public string Title { get; set; }
		public string Text { get; set; }
	}

	public class MessageTarget
	{
		public string UserId { get; set; }
		public string Intent { get; set; }
		public string Locale { get; set; }
		public ActionsArgument Argument { get; set; }
	}

	public class ActionsArgument
    {
        public string  Name { get; set; }
        public string  RawText { get; set; }
    }
}

