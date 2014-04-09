using CodeHub.Core.Services;
using CodeFramework.Core.Services;

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
                return IsActivated(InAppPurchases.PushNotificationsId);
            }
            set
            {
                _defaultValueService.Set(InAppPurchases.PushNotificationsId, value);
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
    }
}

