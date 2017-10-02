using System;
using StoreKit;
using System.Threading.Tasks;
using Foundation;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Linq;
using Splat;
using Plugin.Settings;

namespace CodeHub.iOS.Services
{
    public interface IInAppPurchaseService
    {
        Task<SKProductsResponse> RequestProductData(params string[] productIds);

        Task Restore();

        Task PurchaseProduct(string productId);

        IObservable<Exception> ThrownExceptions { get; }
    }

    public class InAppPurchaseService : IInAppPurchaseService, IEnableLogger
    {
        private readonly TransactionObserver _observer;
        private TaskCompletionSource<bool> _actionSource;
        private TaskCompletionSource<bool> _restoreSource;
        private readonly LinkedList<object> _productDataRequests = new LinkedList<object>();
        private readonly ISubject<Exception> _errorSubject = new Subject<Exception>();

        public IObservable<Exception> ThrownExceptions { get { return _errorSubject; } }

        public InAppPurchaseService()
        {
            _observer = new TransactionObserver(this);
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(_observer);
        }

        public async Task<SKProductsResponse> RequestProductData (params string[] productIds)
        {
            var array = new NSString[productIds.Length];
            for (var i = 0; i < productIds.Length; i++)
                array[i] = new NSString(productIds[i]);

            var tcs = new TaskCompletionSource<SKProductsResponse>();
            _productDataRequests.AddLast(tcs);

            try
            {
                var productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array); //NSSet.MakeNSObjectSet<NSString>(array);​​​
                var productsRequest = new SKProductsRequest(productIdentifiers);
                productsRequest.ReceivedResponse += (sender, e) => tcs.SetResult(e.Response);
                productsRequest.RequestFailed += (sender, e) => tcs.SetException(new Exception(e.Error.LocalizedDescription));
                productsRequest.Start();
                if (await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30))) != tcs.Task)
                    throw new InvalidOperationException("Timeout waiting for Apple to respond");
                var ret = tcs.Task.Result;
                productsRequest.Dispose();
                return ret;
            }
            finally
            {
                _productDataRequests.Remove(tcs);
            }
        }

        public static bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;        
        }

        public Task Restore()
        {
            this.Log().Debug("Preparing to restore purchases");
            _restoreSource = new TaskCompletionSource<bool>();
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
            return _restoreSource.Task;
        }

        public async Task PurchaseProduct(string productId)
        {
            this.Log().Debug("Preparing to purchase: " + productId);
            _actionSource = new TaskCompletionSource<bool>();
            var payment = SKMutablePayment.PaymentWithProduct(productId);
            SKPaymentQueue.DefaultQueue.AddPayment (payment);
            await _actionSource.Task;
        }

        private void CompleteTransaction (SKPaymentTransaction transaction)
        {
            var productId = transaction?.Payment?.ProductIdentifier;
            if (productId == null)
                throw new Exception("Unable to complete transaction as iTunes returned an empty product identifier!");

            CrossSettings.Current.AddOrUpdateValue(productId, true);
            _actionSource?.TrySetResult(true);
        }

        private void RestoreTransaction (SKPaymentTransaction transaction)
        {
            var productId = transaction?.Payment?.ProductIdentifier;
            if (productId == null)
                throw new Exception("Unable to restore transaction as iTunes returned an empty product identifier!");

            CrossSettings.Current.AddOrUpdateValue(productId, true);

            if (productId == FeaturesService.EnterpriseEdition ||
                productId == FeaturesService.PushNotifications)
                Core.Settings.IsProEnabled = true;
        }

        private void FailedTransaction (SKPaymentTransaction transaction)
        {
            var errorString = transaction?.Error?.LocalizedDescription ?? "Unable to process transaction!";
            _actionSource?.TrySetException(new Exception(errorString));
        }

        private void DeferedTransaction()
        {
            const string errorString = "Parental controls are active. After approval, the purchase will be complete.";
            _actionSource?.TrySetException(new Exception(errorString));
        }

        private class TransactionObserver : SKPaymentTransactionObserver, IEnableLogger
        {
            private readonly InAppPurchaseService _inAppPurchases;

            public TransactionObserver(InAppPurchaseService inAppPurchases)
            {
                _inAppPurchases = inAppPurchases;
            }

            public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
            {
                foreach (var transaction in transactions.Where(x => x != null))
                {
                    this.Log().Debug("UpdatedTransactions: " + transaction.TransactionState);

                    try
                    {
                        switch (transaction.TransactionState)
                        {
                            case SKPaymentTransactionState.Purchased:
                                _inAppPurchases.CompleteTransaction(transaction);
                                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                                break;
                            case SKPaymentTransactionState.Failed:
                                _inAppPurchases.FailedTransaction(transaction);
                                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                                break;
                            case SKPaymentTransactionState.Restored:
                                _inAppPurchases.RestoreTransaction(transaction);
                                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                                break;
                            case SKPaymentTransactionState.Deferred:
                                _inAppPurchases.DeferedTransaction();
                                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _inAppPurchases._errorSubject.OnNext(e);
                    }
                }
            }

            public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
            {
                this.Log().Debug("Payment queue restore complete");
                _inAppPurchases._restoreSource?.TrySetResult(true);
            }

            public override void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
            {
                this.Log().Debug("Restore completed with error: " + error);

                if (error.Code == 2)
                {
                    _inAppPurchases._restoreSource?.TrySetCanceled();
                }
                else
                {
                    var errorMessage = error.LocalizedDescription ?? "Unable to restore purchase due to unknown reason.";
                    _inAppPurchases._restoreSource?.TrySetException(new Exception(errorMessage));
                }
            }
        }
    }

    public static class SKProductExtension 
    {
        public static string LocalizedPrice (this SKProduct product)
        {
            var formatter = new NSNumberFormatter ();
            formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;  
            formatter.NumberStyle = NSNumberFormatterStyle.Currency;
            formatter.Locale = product.PriceLocale;
            var formattedString = formatter.StringFromNumber(product.Price);
            return formattedString;
        }
    }
}

