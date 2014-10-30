using MonoTouch.UIKit;

namespace CodeHub.iOS.Themes
{
    public class DefaultTheme : ITheme
    {
        public UIColor NavigationBarColor 
        {
            get { return UIColor.FromRGB(50, 50, 50); }
        }

        public UIColor NavigationBarTextColor
        {
            get { return UIColor.White; }
        }
    }
}

