using System;
using ReactiveUI;

// Analysis disable once CheckNamespace
namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> IsNotNull<T>(this IObservable<T> @this) where T : class
        {
            return @this.Where(x => x != null);
        }

        public static IObservable<TRet> WhenViewModel<TViewModel, TRet>(this IViewFor<TViewModel> @this, 
            System.Linq.Expressions.Expression<Func<TViewModel, TRet>> @select) where TViewModel : class
        {
            return @this.WhenAnyValue(x => x.ViewModel).Where(x => x != null).Select(x => x.WhenAnyValue(@select)).Switch();
        }
    }
}

