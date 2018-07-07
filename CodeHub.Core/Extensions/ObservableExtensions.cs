// Analysis disable once CheckNamespace
namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeError<T>(this IObservable<T> @this, Action<Exception> onError)
        {
            return @this.Subscribe(_ => { }, onError);
        }
    }
}
