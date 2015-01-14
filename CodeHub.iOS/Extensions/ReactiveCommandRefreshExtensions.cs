using System;
using MonoTouch.UIKit;

// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandRefreshExtensions
    {
        public static UIRefreshControl ToRefreshControl(this IReactiveCommand @this)
        {
            var refreshControl = new UIRefreshControl();
            refreshControl.ValueChanged += (sender, e) => @this.ExecuteIfCan();
            @this.IsExecuting.Subscribe(x =>
            {
                if (x)
                    refreshControl.BeginRefreshing();
                else
                    refreshControl.EndRefreshing();
            });
            return refreshControl;
        }

        public static UIBarButtonItem ToBarButtonItem(this IReactiveCommand @this, UIBarButtonSystemItem item)
        {
            var button = new UIBarButtonItem(item, (s, e) => @this.ExecuteIfCan());
            button.EnableIfExecutable(@this);
            return button;
        }
    }
}

