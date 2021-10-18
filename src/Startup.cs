using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(PushNotificationsAPI.Startup))]
namespace PushNotificationsAPI
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(Microsoft.Azure.Functions.Extensions.DependencyInjection.IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
        }
    }
}
