using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using UIKit;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;
        private readonly IInAppPurchaseService _inAppPurchaseService;

        /// <summary>
        /// The pro edition identifier
        /// </summary>
        public const string ProEdition = "com.dillonbuchanan.codehub.pro";
        public const string PushNotifications = "com.dillonbuchanan.codehub.push";
        public const string EnterpriseSupport = "com.dillonbuchanan.codehub.enterprise_support";

        public FeaturesService(IDefaultValueService defaultValueService, IInAppPurchaseService inAppPurchaseService)
        {
            _defaultValueService = defaultValueService;
            _inAppPurchaseService = inAppPurchaseService;
        }

        public bool IsPushNotificationsActivated
        {
            get
            {
                return IsActivated(ProEdition);
            }
        }

        public bool IsEnterpriseSupportActivated
        {
            get
            {
                return IsActivated(ProEdition);
            }
        }

        public bool IsPrivateRepositoriesEnabled
        {
            get
            {
                return IsActivated(ProEdition);
            }
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
            var productData = (await _inAppPurchaseService.RequestProductData(ProEdition)).Products.FirstOrDefault();
            if (productData == null)
                throw new InvalidOperationException("Unable to activate CodeHub Pro");
            await _inAppPurchaseService.PurchaseProduct(productData);
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            appDelegate?.RegisterUserForNotifications();
        }

        public void ActivateProDirect()
        {
            _defaultValueService.Set(ProEdition, true);
        }

        public Task RestorePro()
        {
            return _inAppPurchaseService.Restore();
        }

        private bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet(id, out value) && value;
        }
    }
}

