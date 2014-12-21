using CodeHub.Core.Services;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Xamarin.Utilities.Services;
using System.Net.Http;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;

        public FeaturesService(IDefaultValueService defaultValueService)
        {
            _defaultValueService = defaultValueService;
        }

        public bool IsPushNotificationsActivated
        {
            get
            {
                return IsActivated(FeatureIds.PushNotifications);
            }
            set
            {
                _defaultValueService.Set(FeatureIds.PushNotifications, value);
            }
        }

        public bool IsEnterpriseSupportActivated
        {
            get
            {
                return IsActivated(FeatureIds.EnterpriseSupport);
            }
            set
            {
                _defaultValueService.Set(FeatureIds.EnterpriseSupport, value);
            }
        }

        public void Activate(string id)
        {
            //InAppPurchases.Instance.PurchaseProduct(id);
        }

        public bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet<bool>(id, out value) && value;
        }

        public async Task<IEnumerable<string>> GetAvailableFeatureIds()
        {
            var ids = new List<string>();
            ids.Add(FeatureIds.EnterpriseSupport);
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 15);
            var response = await client.GetAsync("http://push.codehub-app.com/in-app");
            var data = await response.Content.ReadAsStringAsync();
//            ids.AddRange(_jsonSerializationService.Deserialize<List<string>>(data));
            return ids;
        }

        public Task PromptPushNotificationFeature()
        {
//            var tcs = new TaskCompletionSource<object>();
////            var ctrl = IoC.Resolve<EnablePushNotificationsViewController>();
////            ctrl.Dismissed += (sender, e) => tcs.SetResult(null);
//            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
//            if (appDelegate != null)
//                appDelegate.Window.RootViewController.PresentViewController(ctrl, true, null);
//            return tcs.Task;
            throw new NotImplementedException();
        }
    }
}

