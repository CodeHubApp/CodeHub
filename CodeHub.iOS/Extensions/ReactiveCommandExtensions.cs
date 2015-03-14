using System;
using UIKit;

// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
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
            if (@this == null)
                return null;
            var button = new UIBarButtonItem(item, (s, e) => @this.ExecuteIfCan(s));
            button.EnableIfExecutable(@this);
            return button;
        }

        public static UIBarButtonItem ToBarButtonItem(this IReactiveCommand @this, UIImage image)
        {
            if (@this == null)
                return null;
            return new UIBarButtonItem { Image = image }.WithCommand(@this);
        }
    }
}

