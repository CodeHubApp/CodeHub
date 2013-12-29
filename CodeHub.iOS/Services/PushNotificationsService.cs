using System;
using CodeHub.Core.Services;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Services
{
	public class PushNotificationsService : IPushNotificationsService
    {
		private readonly object _lock = new object();

		public void Register()
		{
			lock (_lock)
			{
				var del = (AppDelegate)UIApplication.SharedApplication.Delegate;

				if (string.IsNullOrEmpty(del.DeviceToken))
					throw new InvalidOperationException("Push notifications has not been enabled for this app!");

				var user = Cirrious.CrossCore.Mvx.Resolve<IApplicationService>().Account;

				var client = new RestSharp.RestClient();
				var request = new RestSharp.RestRequest("http://codehub-push.herokuapp.com/register", RestSharp.Method.POST);
				request.AddParameter("token", del.DeviceToken);
				request.AddParameter("user", user.Username);
				request.AddParameter("oauth", user.OAuth);
				request.Timeout = 1000 * 30;
				var response = client.Execute(request);
				if (response.ErrorException != null)
					throw response.ErrorException;

				if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created)
					return;
				throw new InvalidOperationException("Unable to register! Server returned a " + response.StatusCode + " status code");
			}
		}

		public void Deregister()
		{
			lock (_lock)
			{
				var del = (AppDelegate)UIApplication.SharedApplication.Delegate;
				var user = Cirrious.CrossCore.Mvx.Resolve<IApplicationService>().Account;
				var client = new RestSharp.RestClient();
				var request = new RestSharp.RestRequest("http://codehub-push.herokuapp.com/unregister", RestSharp.Method.POST);
				request.AddParameter("token", del.DeviceToken);
				request.AddParameter("oauth", user.OAuth);
				request.Timeout = 1000 * 30;
				var response = client.Execute(request);
				if (response.ErrorException != null)
					throw response.ErrorException;

				if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NotFound)
					return;
				throw new InvalidOperationException("Unable to deregister! Server returned a " + response.StatusCode + " status code");
			}
		}
    }
}

