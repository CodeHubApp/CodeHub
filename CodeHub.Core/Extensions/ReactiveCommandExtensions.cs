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
            if (@this.CanExecute(o))
                @this.Execute(o);
        }

        public static void ExecuteIfCan(this IReactiveCommand @this)
        {
            ExecuteIfCan(@this, null);
        }

        public static IDisposable TriggerNetworkActivity(this IReactiveCommand @this, INetworkActivityService networkActivity)
        {
            return @this.IsExecuting.Skip(1).Subscribe(x =>
            {
                if (x) networkActivity.PushNetworkActive();
                else networkActivity.PopNetworkActive();
            });
        }

        public static IReactiveCommand<object> WithSubscription(this IReactiveCommand<object> @this, Action<object> action)
        {
            @this.Subscribe(action);
            return @this;
        }
    }
}