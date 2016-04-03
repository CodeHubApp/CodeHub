using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS
{
    public class Theme
    {
        public static Theme CurrentTheme { get; private set; }

        private static UIImage CreateBackgroundImage(UIColor color)
        {
            UIGraphics.BeginImageContext(new CoreGraphics.CGSize(1, 1f));
            color.SetFill();
            UIGraphics.RectFill(new CoreGraphics.CGRect(0, 0, 1, 1));
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return img;
        }

        public static void Setup()
        {
            var theme = new Theme();
            CurrentTheme = theme;

            var primaryColor = Theme.CurrentTheme.PrimaryColor;
            var iconColor = UIColor.FromRGB(0x5B, 0x61, 0x65);
            var backgroundImg = CreateBackgroundImage(primaryColor);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            UINavigationBar.Appearance.TintColor = UIColor.White;
            UINavigationBar.Appearance.SetBackgroundImage(backgroundImg, UIBarMetrics.Default);
            UINavigationBar.Appearance.BarTintColor = primaryColor;
            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(18f) });
            CodeHub.iOS.Utilities.Hud.BackgroundTint = UIColor.FromRGBA(228, 228, 228, 128);
            UINavigationBar.Appearance.BackIndicatorImage = Images.Buttons.BackButton;
            UINavigationBar.Appearance.BackIndicatorTransitionMaskImage = Images.Buttons.BackButton;

            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -System.nfloat.MaxValue), UIBarMetrics.LandscapePhone);
            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -System.nfloat.MaxValue), UIBarMetrics.Default);

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = UIColor.FromRGB(50, 50, 50);

            UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TextColor = UIColor.FromRGB(136, 136, 136);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).Font = UIFont.SystemFontOfSize(13f);

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);

            UIImageView.AppearanceWhenContainedIn(typeof(UITableViewCell), typeof(MonoTouch.SlideoutNavigation.MainNavigationController)).TintColor = iconColor;

            EmptyListView.DefaultColor = primaryColor;
            LoadingIndicatorView.DefaultColor = primaryColor;
        }

        public UITextAttributes SegmentedControlText
        {
            get
            {
                return new UITextAttributes
                { 
                    Font = UIFont.PreferredBody, 
                    TextColor = UIColor.FromRGB(87, 85, 85), 
                    TextShadowColor = UIColor.FromRGBA(255, 255, 255, 125), 
                    TextShadowOffset = new UIOffset(0, 1) 
                };
            }
        }

        public UIColor ViewBackgroundColor { get { return UIColor.FromRGB(238, 238, 238); } }
        public UIColor MainTitleColor { get { return UIColor.FromRGB(0x41, 0x83, 0xc4); } }
        public UIColor MainSubtitleColor { get { return UIColor.FromRGB(81, 81, 81); } }
        public UIColor MainTextColor { get { return UIColor.FromRGB(41, 41, 41); } }
        public UIColor IssueTitleColor { get { return MainTitleColor; } }
        public UIColor RepositoryTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewDetailColor { get { return MainSubtitleColor; } }
        public UIColor WebButtonTint { get { return UIColor.FromRGB(127, 125, 125); } }
        public UIColor PrimaryColor { get { return UIColor.FromRGB(50, 50, 50); } }
    }
}
