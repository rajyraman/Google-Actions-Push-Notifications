using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace PushNotificationsAPI.Actors
{
    public interface IPushNotifications
    {
        void Subscribe(PushNotification subscription);
        void UnSubscribe(string userId);
        Task<ReadOnlyCollection<PushNotification>> GetSubscriptions();

        void Clear();
    }
}
