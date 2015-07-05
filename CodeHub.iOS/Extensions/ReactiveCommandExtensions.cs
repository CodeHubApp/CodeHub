using System;
using UIKit;

// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveCommandExtensions
    {
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

