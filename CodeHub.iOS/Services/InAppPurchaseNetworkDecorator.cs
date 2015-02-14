using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public class InAppPurchaseNetworkDecorator : IInAppPurchaseService
    {
        private readonly IInAppPurchaseService _inAppPurcahseService;
        private readonly INetworkActivityService _networkActivity;

        public InAppPurchaseNetworkDecorator(IInAppPurchaseService inAppPurcahseService, INetworkActivityService networkActivity)
        {
            _inAppPurcahseService = inAppPurcahseService;
            _networkActivity = networkActivity;
        }

        public System.Threading.Tasks.Task<StoreKit.SKProductsResponse> RequestProductData(params string[] productIds)
        {
            using (_networkActivity.ActivateNetwork())
                return _inAppPurcahseService.RequestProductData(productIds);
        }

        public System.Threading.Tasks.Task Restore()
        {
            using (_networkActivity.ActivateNetwork())
                return _inAppPurcahseService.Restore();
        }

        public System.Threading.Tasks.Task PurchaseProduct(StoreKit.SKProduct productId)
        {
            using (_networkActivity.ActivateNetwork())
                return _inAppPurcahseService.PurchaseProduct(productId);
        }
    }
}

