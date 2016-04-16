using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using UIKit;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;
        private readonly IInAppPurchaseService _inAppPurchaseService;
   
        public const string ProEdition = "com.dillonbuchanan.codehub.pro";
        public const string EnterpriseEdition = "com.dillonbuchanan.codehub.enterprise_support";
        public const string PushNotifications = "com.dillonbuchanan.codehub.push";

        public FeaturesService(IDefaultValueService defaultValueService, IInAppPurchaseService inAppPurchaseService)
        {
            _defaultValueService = defaultValueService;
            _inAppPurchaseService = inAppPurchaseService;
        }

        public bool IsProEnabled
        {
            get
            {
                return IsActivated(ProEdition);
            }
        }

        public async Task ActivatePro()
        {
            await _inAppPurchaseService.PurchaseProduct(ProEdition);
            ActivateUserNotifications();
        }

        public void ActivateProDirect()
        {
            _defaultValueService.Set(ProEdition, true);
        }

        public async Task RestorePro()
        {
            await _inAppPurchaseService.Restore();
            ActivateUserNotifications();
        }

        private bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet(id, out value) && value;
        }

        private void ActivateUserNotifications()
        {
            if (IsProEnabled)
            {
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                appDelegate?.RegisterUserForNotifications();
            }
        }
    }
}

