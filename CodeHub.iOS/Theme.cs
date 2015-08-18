using UIKit;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.ViewControllers;
using System.Linq;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS
{
    public static class Theme
    {
        public static UIColor PrimaryNavigationBarColor
        {
            get { return UIColor.FromRGB(50, 50, 50); }
        }

        public static UIColor PrimaryNavigationBarTextColor
        {
            get { return UIColor.White; }
        }

        public static UIColor MainTextColor 
        { 
            get { return UIColor.FromRGB(41, 41, 41); } 
        }

        public static UIColor MainTitleColor 
        { 
            get { return UIColor.FromRGB(0x41, 0x83, 0xc4); } 
        }

        public static UIColor MainSubtitleColor 
        { 
            get { return UIColor.FromRGB(81, 81, 81); } 
        }

        public static UIColor IconColor
        {
            get { return UIColor.FromRGB(0x32, 0x32, 0x32); }
        }

        public static UIColor MenuIconColor
        {
            get { return UIColor.FromRGB(0xd5, 0xd5, 0xd5); }
        }

        public static UIColor MenuTextColor
        {
            get { return UIColor.FromRGB(0xd5, 0xd5, 0xd5); }
        }

        public static UIColor MenuBackgroundColor
        {
            get { return UIColor.FromRGB(0x22, 0x22, 0x22); }
        }

        public static UIColor MenuSelectedBackgroundColor
        {
            get { return UIColor.FromRGB(0x19, 0x19, 0x19); }
        }

        public static void Setup()
        {
            UIGraphics.BeginImageContext(new CoreGraphics.CGSize(1, 64f));
            Theme.PrimaryNavigationBarColor.SetFill();
            UIGraphics.RectFill(new CoreGraphics.CGRect(0, 0, 1, 64));
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            var navBarContainers = new [] { typeof(MenuNavigationController), typeof(ThemedNavigationController), typeof(MainNavigationController) };
            foreach (var navbarAppearance in navBarContainers.Select(x => UINavigationBar.AppearanceWhenContainedIn(x)))
            {
                navbarAppearance.TintColor = Theme.PrimaryNavigationBarTextColor;
                navbarAppearance.BarTintColor = Theme.PrimaryNavigationBarColor;
                navbarAppearance.BackgroundColor = Theme.PrimaryNavigationBarColor;
                navbarAppearance.SetTitleTextAttributes(new UITextAttributes { TextColor = Theme.PrimaryNavigationBarTextColor, Font = UIFont.SystemFontOfSize(18f) });
                navbarAppearance.SetBackgroundImage(img, UIBarPosition.Any, UIBarMetrics.Default);
                navbarAppearance.BackIndicatorImage = Images.BackButton;
                navbarAppearance.BackIndicatorTransitionMaskImage = Images.BackButton;
            }

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            UISwitch.Appearance.OnTintColor = UIColor.FromRGB(0x41, 0x83, 0xc4);

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = Theme.PrimaryNavigationBarColor;

            //UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            var headerFooterContainers = new [] { typeof(UITableViewHeaderFooterView) };
            foreach (var navbarAppearance in headerFooterContainers)
            {
                UILabel.AppearanceWhenContainedIn(navbarAppearance).TextColor = UIColor.FromRGB(110, 110, 117);
                UILabel.AppearanceWhenContainedIn(navbarAppearance).Font = UIFont.SystemFontOfSize(14f);
            }

            StringElement.DefaultTintColor = Theme.PrimaryNavigationBarColor;

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);
        }
    }
}
