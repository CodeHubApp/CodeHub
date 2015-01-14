
// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq
{
    using System;

    public static class ReactiveExtensions
    {
        public static IObservable<TResult> SelectSafe<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            return source.Select(selector).Catch(Observable.Empty<TResult>());
        }
    }
}

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

