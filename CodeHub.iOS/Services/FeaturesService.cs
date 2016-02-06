using CodeHub.Core.Services;
using CodeHub.Core.Services;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Foundation;
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
            InAppPurchases.Instance.PurchaseProduct(id);
        }

        public bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet<bool>(id, out value) && value;
        }

        public async Task<IEnumerable<string>> GetAvailableFeatureIds()
        {
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 15);

            var response = await client.GetAsync("https://raw.githubusercontent.com/thedillonb/CodeHub/gh-pages/features.json");
            var data = await response.Content.ReadAsStringAsync();
            var features = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(data);

            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            if (!features.ContainsKey(version))
                return new [] { FeatureIds.EnterpriseSupport };
            return features[version];
        }
    }
}

