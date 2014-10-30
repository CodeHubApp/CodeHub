using System;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Themes
{
    public interface ITheme
    {
        UIColor NavigationBarColor { get; }

        UIColor NavigationBarTextColor { get; }
    }
}

