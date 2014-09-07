using System;

namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> IsNotNull<T>(this IObservable<T> @this) where T : class
        {
            return @this.Where(x => x != null);
        }
    }
}

