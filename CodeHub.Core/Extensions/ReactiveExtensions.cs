// ReSharper disable once CheckNamespace
namespace System
{
    public static class ReactiveExtensions
    {
        public static IDisposable SubscribeSafe<T>(this IObservable<T> source, Action<T> onNext)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }

            return source.Subscribe(x =>
            {
                try
                {
                    onNext(x);
                }
                catch
                {
                }
            });
        }
    }
}

