using CodeHub.Core.Services;
using System.Threading.Tasks;
using UIKit;
using Plugin.Settings.Abstractions;
using Plugin.Settings;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly ISettings _defaultValueService;
        private readonly IInAppPurchaseService _inAppPurchaseService;
   
        public const string ProEdition = "com.dillonbuchanan.codehub.pro";
        public const string EnterpriseEdition = "com.dillonbuchanan.codehub.enterprise_support";
        public const string PushNotifications = "com.dillonbuchanan.codehub.push";

        public FeaturesService(IInAppPurchaseService inAppPurchaseService)
        {
            _defaultValueService = CrossSettings.Current;
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
            _defaultValueService.AddOrUpdateValue(ProEdition, true);
        }

        public async Task RestorePro()
        {
            await _inAppPurchaseService.Restore();
            ActivateUserNotifications();
        }

        private bool IsActivated(string id)
        {
            return _defaultValueService.GetValueOrDefault(id, false);
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

