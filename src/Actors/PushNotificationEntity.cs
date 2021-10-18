using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PushNotificationsAPI.Actors
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationEntity : IPushNotifications
    {
        [JsonProperty("subscriptions")]
        private List<PushNotification> notificationSubscriptions { get; set; } = new List<PushNotification>();
        public void Subscribe(PushNotification subscription)
        {
            if (!notificationSubscriptions.Any(x => x.Location == subscription.Location && x.UserId == subscription.UserId))
            {
                notificationSubscriptions.Add(subscription);
            }
        }

        public Task<ReadOnlyCollection<PushNotification>> GetSubscriptions()
        {
            return Task.FromResult(notificationSubscriptions.AsReadOnly());
        }

        public void UnSubscribe(string userId)
        {
            notificationSubscriptions.RemoveAll(x => x.UserId == userId);
        }

        public void Clear()
        {
            notificationSubscriptions.Clear();
        }

        [FunctionName(nameof(PushNotificationEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
               => ctx.DispatchAsync<PushNotificationEntity>();
    }
}
