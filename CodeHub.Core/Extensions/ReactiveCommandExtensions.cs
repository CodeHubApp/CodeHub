using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static void ExecuteIfCan(this IReactiveCommand @this, object o)
        {
            if (@this == null)
                return;

            if (@this.CanExecute(o))
                @this.Execute(o);
        }

        public static void ExecuteIfCan(this IReactiveCommand @this)
        {
            ExecuteIfCan(@this, null);
        }

        public static IReactiveCommand<object> WithSubscription(this IReactiveCommand<object> @this, Action<object> action)
        {
            @this.Subscribe(action);
            return @this;
        }
    }
}