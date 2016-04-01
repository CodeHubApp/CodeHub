using System;
using CodeHub.Core.Services;
using UIKit;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Platform;
using ObjCRuntime;

namespace CodeHub.iOS.Services
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private const string RegisterUri = "https://push.codehub-app.com/register";
        private const string DeregisterUri = "https://push.codehub-app.com/unregister";

        public async Task Register()
        {
            if (Runtime.Arch == Arch.SIMULATOR)
                return;

            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (string.IsNullOrEmpty(appDelegate.DeviceToken))
            {
                appDelegate.RegisterUserForNotifications();
                for (var i = 0; i < 5; i++)
                {
                    if (!string.IsNullOrEmpty(appDelegate.DeviceToken))
                        break;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            if (string.IsNullOrEmpty(appDelegate.DeviceToken))
                throw new InvalidOperationException("Unable to activate push notifications. Please check your iOS notifications settings.");

            var applicationService = Mvx.Resolve<IApplicationService>();
            var user = applicationService.Account;
            if (user.IsEnterprise)
                throw new InvalidOperationException("Push notifications are for GitHub.com accounts only!");

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("token", appDelegate.DeviceToken),
                new KeyValuePair<string, string>("user", user.Username),
                new KeyValuePair<string, string>("domain", "https://api.github.com"),
                new KeyValuePair<string, string>("oauth", user.OAuth)
            });

            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.PostAsync(RegisterUri, content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Conflict)
                throw new InvalidOperationException("Unable to register! Server returned a " + response.StatusCode + " status code");
        }

        public async Task Deregister()
        {
            var del = (AppDelegate)UIApplication.SharedApplication.Delegate;

            if (string.IsNullOrEmpty(del.DeviceToken))
                return;

            var user = Mvx.Resolve<IApplicationService>().Account;
            if (user.IsEnterprise)
                throw new InvalidOperationException("Push notifications are for GitHub.com accounts only!");

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("token", del.DeviceToken),
                new KeyValuePair<string, string>("oauth", user.OAuth),
                new KeyValuePair<string, string>("domain", "https://api.github.com"),
            });

            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.PostAsync(DeregisterUri, content);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound && response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException("Unable to deregister! Server returned a " + response.StatusCode + " status code");
        }
    }
}

