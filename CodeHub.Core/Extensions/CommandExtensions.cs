using System;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static void ExecuteIfCan(this ICommand @this, object o)
        {
            if (@this == null)
                return;

            if (@this.CanExecute(o))
                @this.Execute(o);
        }

        public static void ExecuteIfCan(this ICommand @this)
        {
            ExecuteIfCan(@this, null);
        }

        public static IReactiveCommand<T> WithSubscription<T>(this IReactiveCommand<T> @this, Action<T> action)
        {
            @this.Subscribe(action);
            return @this;
        }
    }
}