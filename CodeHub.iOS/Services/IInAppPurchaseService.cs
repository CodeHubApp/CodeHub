using System;
using StoreKit;
using System.Threading.Tasks;

namespace CodeHub.iOS.Services
{
    public interface IInAppPurchaseService
    {
        Task<SKProductsResponse> RequestProductData(params string[] productIds);

        Task Restore();

        Task PurchaseProduct(SKProduct productId);
    }
}

