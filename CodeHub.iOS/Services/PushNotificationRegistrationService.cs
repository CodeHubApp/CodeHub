using System;
using CodeHub.Core.Services;
using UIKit;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.iOS.Services
{
	public class PushNotificationRegistrationService : IPushNotificationRegistrationService
    {
	    private const string RegisterUri = "http://162.243.15.10/register";
        private const string DeregisterUri = "http://162.243.15.10/unregister";

        public async Task Register(GitHubAccount account)
		{
			var del = (AppDelegate)UIApplication.SharedApplication.Delegate;

			if (string.IsNullOrEmpty(del.DeviceToken))
				throw new InvalidOperationException("Push notifications has not been enabled for this app!");
                
            if (account.IsEnterprise)
                throw new InvalidOperationException("Push notifications are for GitHub.com accounts only!");

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("token", del.DeviceToken),
                new KeyValuePair<string, string>("user", account.Username),
                new KeyValuePair<string, string>("domain", "https://api.github.com"),
                new KeyValuePair<string, string>("oauth", account.OAuth)
            });

            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.PostAsync(RegisterUri, content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Conflict)
                throw new InvalidOperationException("Unable to register! Server returned a " + response.StatusCode + " status code");
            System.Diagnostics.Debug.WriteLine("Push notifications registered for: " + account.Username + " (" + account.OAuth + ") on device <" + del.DeviceToken + ">");
		}

        public async Task Deregister(GitHubAccount account)
		{
			var del = (AppDelegate)UIApplication.SharedApplication.Delegate;

            if (string.IsNullOrEmpty(del.DeviceToken))
                throw new InvalidOperationException("Push notifications has not been enabled for this app!");

            if (account.IsEnterprise)
                throw new InvalidOperationException("Push notifications are for GitHub.com accounts only!");

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("token", del.DeviceToken),
                new KeyValuePair<string, string>("oauth", account.OAuth),
                new KeyValuePair<string, string>("domain", "https://api.github.com")
            });

            client.Timeout = new TimeSpan(0, 0, 30);
            var response = await client.PostAsync(DeregisterUri, content);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound && response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException("Unable to deregister! Server returned a " + response.StatusCode + " status code");
            System.Diagnostics.Debug.WriteLine("Push notifications deregistered for: " + account.Username + " (" + account.OAuth + ") on device <" + del.DeviceToken + ">");
		}
    }
}

