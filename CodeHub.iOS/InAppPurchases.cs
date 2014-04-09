using System;
using MonoTouch.StoreKit;
using MonoTouch.Foundation;
using System.Threading.Tasks;

namespace CodeHub.iOS
{
    public class InAppPurchases 
    {
        private static InAppPurchases _instance;
        private readonly TransactionObserver _observer;

        public event EventHandler<string> PurchaseSuccess;
        public event EventHandler<Exception> PurchaseError;

        public static InAppPurchases Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InAppPurchases();
                return _instance;
            }
        }

        private void OnPurchaseError(Exception e)
        {
            var handle = PurchaseError;
            if (handle != null)
                handle(this, e);
        }

        private void OnPurchaseSuccess(string id)
        {
            var handle = PurchaseSuccess;
            if (handle != null)
                handle(this, id);
        }

        private InAppPurchases()
        {
            _observer = new TransactionObserver(this);
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(_observer);
        }

        public static async Task<SKProductsResponse> RequestProductData (params string[] productIds)
        {
            var array = new NSString[productIds.Length];
            for (var i = 0; i < productIds.Length; i++)
                array[i] = new NSString(productIds[i]);

            var tcs = new TaskCompletionSource<SKProductsResponse>();
            var productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array); //NSSet.MakeNSObjectSet<NSString>(array);​​​
            var productsRequest = new SKProductsRequest(productIdentifiers);
            productsRequest.ReceivedResponse += (sender, e) => tcs.SetResult(e.Response);
            productsRequest.RequestFailed += (sender, e) => tcs.SetException(new Exception(e.Error.LocalizedDescription));
            productsRequest.Start();
            return await tcs.Task;
        }

        public static bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;        
        }

        public void Restore()
        {
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();                        
        }

        public void PurchaseProduct(string productId)
        {
            SKPayment payment = SKPayment.PaymentWithProduct(productId);
            SKPaymentQueue.DefaultQueue.AddPayment (payment);
        }

        private void CompleteTransaction (SKPaymentTransaction transaction)
        {
            Console.WriteLine ("CompleteTransaction " + transaction.TransactionIdentifier);
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
            OnPurchaseSuccess(transaction.Payment.ProductIdentifier);
        }

        private void RestoreTransaction (SKPaymentTransaction transaction)
        {
            Console.WriteLine("RestoreTransaction " + transaction.TransactionIdentifier + "; OriginalTransaction " + transaction.OriginalTransaction.TransactionIdentifier);
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
            OnPurchaseSuccess(transaction.OriginalTransaction.Payment.ProductIdentifier);
        }

        private void FailedTransaction (SKPaymentTransaction transaction)
        {
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
            OnPurchaseError(new Exception(transaction.Error.LocalizedDescription));
        }

        private class TransactionObserver : SKPaymentTransactionObserver
        {
            private readonly InAppPurchases _inAppPurchases;

            public TransactionObserver(InAppPurchases inAppPurchases)
            {
                _inAppPurchases = inAppPurchases;
            }

            public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
            {
                Console.WriteLine ("UpdatedTransactions");
                foreach (SKPaymentTransaction transaction in transactions)
                {
                    switch (transaction.TransactionState)
                    {
                        case SKPaymentTransactionState.Purchased:
                            _inAppPurchases.CompleteTransaction(transaction);
                            break;
                        case SKPaymentTransactionState.Failed:
                            _inAppPurchases.FailedTransaction(transaction);
                            break;
                        case SKPaymentTransactionState.Restored:
                            _inAppPurchases.RestoreTransaction(transaction);
                            break;
                        default:
                            break;
                    }
                }
            }

            public override void PaymentQueueRestoreCompletedTransactionsFinished (SKPaymentQueue queue)
            {
                Console.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
            }

            public override void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
            {
                Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
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

