using System;
using MonoTouch.UIKit;
using ReactiveUI;

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
}

