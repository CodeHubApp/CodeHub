using System;

// Analysis disable once CheckNamespace
namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> IsNotNull<T>(this IObservable<T> @this) where T : class
        {
            return @this.Where(x => x != null);
        }

        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return Observable.Create<Unit>(observer =>
                {
                    Action onCompleted = () =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    };
                    return observable.Subscribe(_ => {}, observer.OnError, onCompleted);
                });
        }
    }
}

