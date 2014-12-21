using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;
using CodeHub.iOS.ViewControllers;
using MonoTouch.SlideoutNavigation;
using System.Linq;
using Splat;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS
{
    public class Theme
    {
        public static Theme CurrentTheme { get; private set; }

        public static void Setup()
        {
            var theme = new Theme();
            CurrentTheme = theme;

            var defaultValues = Locator.Current.GetService<IDefaultValueService>();

            bool largeFonts;
            if (!defaultValues.TryGet("large_fonts", out largeFonts))
                largeFonts = false;
            Theme.CurrentTheme.FontSizeRatio = largeFonts ? 1.3f : 1.0f;

            UIGraphics.BeginImageContext(new System.Drawing.SizeF(1, 64f));
            UIColor.FromRGB(50, 50, 50).SetFill();
            UIGraphics.RectFill(new System.Drawing.RectangleF(0, 0, 1, 64));
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            //Element.FontSizeRatio = Theme.CurrentTheme.FontSizeRatio;

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            var navBarContainers = new [] { typeof(MenuNavigationController), typeof(ThemedNavigationController), typeof(MainNavigationController) };
            foreach (var navbarAppearance in navBarContainers.Select(x => UINavigationBar.AppearanceWhenContainedIn(x)))
            {
                navbarAppearance.TintColor = PrimaryNavigationBarTextColor;
                navbarAppearance.BarTintColor = PrimaryNavigationBarColor;
                navbarAppearance.BackgroundColor = PrimaryNavigationBarColor;
                navbarAppearance.SetTitleTextAttributes(new UITextAttributes { TextColor = PrimaryNavigationBarTextColor, Font = UIFont.SystemFontOfSize(18f) });
                navbarAppearance.SetBackgroundImage(img, UIBarPosition.Any, UIBarMetrics.Default);
                //CodeFramework.iOS.Utils.Hud.BackgroundTint = UIColor.FromRGBA(228, 228, 228, 128);
                navbarAppearance.BackIndicatorImage = Images.BackButton;
                navbarAppearance.BackIndicatorTransitionMaskImage = Images.BackButton;
            }


//            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(-1000, -64), UIBarMetrics.LandscapePhone);
//            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(-1000, -64), UIBarMetrics.Default);
//
//            UserVoice.UVStyleSheet.Instance.NavigationBarTintColor = UIColor.White;
//            UserVoice.UVStyleSheet.Instance.NavigationBarTextColor = UIColor.White;

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = PrimaryNavigationBarColor;

            UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TextColor = UIColor.FromRGB(136, 136, 136);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).Font = UIFont.SystemFontOfSize(13f * Theme.CurrentTheme.FontSizeRatio);

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);

//            CodeFramework.Elements.NewsFeedElement.LinkColor = theme.MainTitleColor;
//            CodeFramework.Elements.NewsFeedElement.TextColor = theme.MainTextColor;
//            CodeFramework.Elements.NewsFeedElement.NameColor = theme.MainTitleColor;
        }

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

        public UITextAttributes SegmentedControlText
        {
            get
            {
                return new UITextAttributes
                { 
                    Font = UIFont.SystemFontOfSize(14f * Theme.CurrentTheme.FontSizeRatio), 
                    TextColor = UIColor.FromRGB(87, 85, 85), 
                    TextShadowColor = UIColor.FromRGBA(255, 255, 255, 125), 
                    TextShadowOffset = new UIOffset(0, 1) 
                };
            }
        }

        public UIImage CheckButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/check"); } }
        public UIImage ThreeLinesButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
        public UIImage SortButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/sort"); } }
        public UIImage SaveButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        public UIImage ViewButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/view"); } }
        public UIImage ForkButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/fork"); } }
        public UIImage WebBackButton { get { return UIImageHelper.FromFileAuto("Images/Web/back"); } }
        public UIImage WebFowardButton { get { return UIImageHelper.FromFileAuto("Images/Web/forward"); } }

        public UIImage AnonymousUserImage { get { return Images.Anonymous; } }

        public UIColor ViewBackgroundColor { get { return UIColor.FromRGB(238, 238, 238); } }


        //Cache these because we make a smaller size of them
        private UIImage _issueCell1, _issueCell2, _issueCell3, _issueCell4;
        private UIImage _repoCell1, _repoCell2, _repoCell3;

        public UIImage IssueCellImage1
        {
            get { return _issueCell1 ?? (_issueCell1 = new UIImage(Images.Cog.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage2
        {
            get { return _issueCell2 ?? (_issueCell2 = new UIImage(Images.Comments.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage3
        {
            get { return _issueCell3 ?? (_issueCell3 = new UIImage(Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage4
        {
            get { return _issueCell4 ?? (_issueCell4 = new UIImage(Images.Pencil.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellFollowers
        {
            get { return _repoCell1 ?? (_repoCell1 = new UIImage(Images.Star.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellForks
        {
            get { return _repoCell2 ?? (_repoCell2 = new UIImage(Images.Fork.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellUser
        {
            get { return _repoCell3 ?? (_repoCell3 = new UIImage(Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIColor NavigationTextColor { get { return UIColor.FromRGB(97, 95, 95); } }

        public UIColor IssueTitleColor { get { return MainTitleColor; } }
        public UIColor RepositoryTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewDetailColor { get { return MainSubtitleColor; } }

        public UIColor WebButtonTint { get { return UIColor.FromRGB(127, 125, 125); } }

        public UIColor AccountsNavigationBarTint
        {
            get
            {
                return UIColor.Red;
            }
        }

        public UIColor SlideoutNavigationBarTint
        {
            get
            {
                return UIColor.Black;
            }
        }

        public UIColor ApplicationNavigationBarTint
        {
            get
            {
                return UIColor.Black;
            }
        }

        public float FontSizeRatio { get; set; }

        public UIImage LoginUserUnknown { get { return Images.LoginUserUnknown; } }
    }
}
