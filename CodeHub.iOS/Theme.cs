using UIKit;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.ViewControllers;
using System.Linq;
using CodeHub.iOS.Views;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CodeHub.iOS
{
    public static class Theme
    {
        public static UIColor PrimaryNavigationBarColor = UIColor.FromRGB(50, 50, 50);

        public static UIColor PrimaryNavigationBarTextColor = UIColor.White;

        public static UIColor MainTitleColor = UIColor.FromRGB(0x41, 0x83, 0xc4);

        public static UIColor PrimaryMenuNavigationBarColor
        {
            get { return UIColor.FromRGB(50, 50, 50); }
        }

        public static UIColor PrimaryMenuNavigationBarTextColor
        {
            get { return UIColor.White; }
        }

        public static UIColor MainTextColor 
        { 
            get { return UIColor.FromRGB(41, 41, 41); } 
        }

        public static UIColor MainSubtitleColor 
        { 
            get { return UIColor.FromRGB(81, 81, 81); } 
        }

        public static UIColor MenuIconColor
        {
            get { return UIColor.FromRGB(0xd5, 0xd5, 0xd5); }
        }

        public static UIColor MenuSectionTextColor
        {
            get { return UIColor.FromRGB(171, 171, 171); }
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

        private static UIImage CreateBackgroundImage(UIColor color)
        {
            UIGraphics.BeginImageContext(new CoreGraphics.CGSize(1, 64f));
            color.SetFill();
            UIGraphics.RectFill(new CoreGraphics.CGRect(0, 0, 1, 64));
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return img;
        }

        public static void Setup()
        {
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            var menuNavBarBackground = CreateBackgroundImage(Theme.PrimaryMenuNavigationBarColor);
            var menuNavBarContainer = UINavigationBar.AppearanceWhenContainedIn(typeof(MenuNavigationController));
            menuNavBarContainer.TintColor = Theme.PrimaryMenuNavigationBarTextColor;
            menuNavBarContainer.BarTintColor = Theme.PrimaryMenuNavigationBarColor;
            menuNavBarContainer.BackgroundColor = Theme.PrimaryMenuNavigationBarColor;
            menuNavBarContainer.SetTitleTextAttributes(new UITextAttributes { TextColor = Theme.PrimaryMenuNavigationBarTextColor, Font = UIFont.SystemFontOfSize(18f) });
            menuNavBarContainer.SetBackgroundImage(menuNavBarBackground, UIBarPosition.Any, UIBarMetrics.Default);
            menuNavBarContainer.BackIndicatorImage = Images.BackButton;
            menuNavBarContainer.BackIndicatorTransitionMaskImage = Images.BackButton;

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = Theme.PrimaryNavigationBarColor;

            //UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            var headerFooterContainers = new [] { typeof(UITableViewHeaderFooterView) };
            foreach (var navbarAppearance in headerFooterContainers)
            {
                UILabel.AppearanceWhenContainedIn(navbarAppearance).TextColor = UIColor.FromRGB(110, 110, 117);
                UILabel.AppearanceWhenContainedIn(navbarAppearance).Font = UIFont.SystemFontOfSize(14f);
            }

            MenuSectionView.DefaultBackgroundColor = Theme.PrimaryMenuNavigationBarColor;
            MenuSectionView.DefaultTextColor = Theme.MenuSectionTextColor;

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);
        }

        public class ThemeColors
        {
            public UIColor Primary { get; private set; }
            public UIColor Icon { get; private set; }
            public UIColor Secondary { get; private set; }
            public ThemeColors(UIColor primary, UIColor icon, UIColor secondary)
            {
                Primary = primary;
                Icon = icon;
                Secondary = secondary;
            }
        }

        public readonly static ThemeColors Default = new ThemeColors(
            UIColor.FromRGB(50, 50, 50), 
            UIColor.FromRGB(100, 100, 100), 
            UIColor.FromRGB(0x41, 0x83, 0xc4));

//        public static readonly IDictionary<string, ThemeColors> Themes = new Dictionary<string, ThemeColors>(StringComparer.OrdinalIgnoreCase)
//        {
//            {"blue", new ThemeColors(UIColor.FromRGB(0x1D, 0x62, 0xF0), UIColor.FromRGB(0x1A, 0xD6, 0xFD))},
//            {"red", new ThemeColors(UIColor.FromRGB(231, 76, 60), UIColor.FromRGB(192, 57, 43))},
//        };
// 
        public static void SetPrimary(string colorName)
        {
            colorName = colorName ?? string.Empty;
            var primaryColor = Default.Primary;
            var secondaryColor = Default.Secondary;
            var iconColor = Default.Icon;

//            if (Themes.ContainsKey(colorName))
//            {
//                var a = Themes[colorName];
//                primaryColor = a.Primary;
//                secondaryColor = a.Secondary;
//            }

            PrimaryNavigationBarColor = primaryColor;
            MainTitleColor = secondaryColor;
            PrimaryNavigationBarTextColor = UIColor.White;

            var navBarBackground = CreateBackgroundImage(primaryColor);
            var navBarContainers = new [] { typeof(ThemedNavigationController), typeof(MainNavigationController) };
            foreach (var navbarAppearance in navBarContainers.Select(x => UINavigationBar.AppearanceWhenContainedIn(x)))
            {
                navbarAppearance.TintColor = UIColor.White;
                navbarAppearance.BarTintColor = primaryColor;
                navbarAppearance.BackgroundColor = primaryColor;
                navbarAppearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(18f) });
                navbarAppearance.SetBackgroundImage(navBarBackground, UIBarPosition.Any, UIBarMetrics.Default);
                navbarAppearance.BackIndicatorImage = Images.BackButton;
                navbarAppearance.BackIndicatorTransitionMaskImage = Images.BackButton;
            }

            UISwitch.Appearance.OnTintColor = secondaryColor;
            UIImageView.AppearanceWhenContainedIn(typeof(UITableViewCell), typeof(MainNavigationController)).TintColor = iconColor;
            LoadingIndicatorView.DefaultColor = primaryColor;
        }
    }
}
