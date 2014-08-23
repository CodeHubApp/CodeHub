using System;
using ReactiveUI;

namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
        public static IReactiveCommand<object> WithSubscription(this IReactiveCommand<object> @this, Action<object> action)
        {
            @this.Subscribe(action);
            return @this;
        }
    }
}

