using UIKit;
using MonoTouch.UIKit;

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

            var defaultValues = Cirrious.CrossCore.Mvx.Resolve<CodeFramework.Core.Services.IDefaultValueService>();

            var primaryColor = UIColor.FromRGB(50, 50, 50);
            var iconColor = UIColor.FromRGB(0x5B, 0x61, 0x65);
            var backgroundImg = CreateBackgroundImage(primaryColor);

            bool largeFonts;
            if (!defaultValues.TryGet<bool>("large_fonts", out largeFonts))
                largeFonts = false;
            Theme.CurrentTheme.FontSizeRatio = largeFonts ? 1.3f : 1.0f;

            MonoTouch.Dialog.NameTimeStringElement.NameColor = Theme.CurrentTheme.MainTitleColor;
            MonoTouch.Dialog.Element.FontSizeRatio = Theme.CurrentTheme.FontSizeRatio;

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            UINavigationBar.Appearance.TintColor = UIColor.White;
            UINavigationBar.Appearance.SetBackgroundImage(backgroundImg, UIBarMetrics.Default);
            UINavigationBar.Appearance.BarTintColor = primaryColor;
            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(18f) });
            CodeFramework.iOS.Utils.Hud.BackgroundTint = UIColor.FromRGBA(228, 228, 228, 128);
            UINavigationBar.Appearance.BackIndicatorImage = Theme.CurrentTheme.BackButton;
            UINavigationBar.Appearance.BackIndicatorTransitionMaskImage = Theme.CurrentTheme.BackButton;

            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -64), UIBarMetrics.LandscapePhone);
            UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -64), UIBarMetrics.Default);

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = UIColor.FromRGB(50, 50, 50);

            UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TextColor = UIColor.FromRGB(136, 136, 136);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).Font = UIFont.SystemFontOfSize(13f * Theme.CurrentTheme.FontSizeRatio);

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);

            UIImageView.AppearanceWhenContainedIn(typeof(UITableViewCell), typeof(MonoTouch.SlideoutNavigation.MainNavigationController)).TintColor = iconColor;

//            CodeFramework.Elements.NewsFeedElement.LinkColor = theme.MainTitleColor;
//            CodeFramework.Elements.NewsFeedElement.TextColor = theme.MainTextColor;
//            CodeFramework.Elements.NewsFeedElement.NameColor = theme.MainTitleColor;
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
        public UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/back"); } }
        public UIImage ThreeLinesButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
        public UIImage CancelButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/cancel"); } }
        public UIImage SortButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/sort"); } }
        public UIImage SaveButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        public UIImage ViewButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/view"); } }
        public UIImage ForkButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/fork"); } }
        public UIImage WebBackButton { get { return UIImageHelper.FromFileAuto("Images/Web/back"); } }
        public UIImage WebFowardButton { get { return UIImageHelper.FromFileAuto("Images/Web/forward"); } }

        public UIColor ViewBackgroundColor { get { return UIColor.FromRGB(238, 238, 238); } }


        //Cache these because we make a smaller size of them
        private UIImage _issueCell1, _issueCell2, _issueCell3, _issueCell4;
        private UIImage _repoCell1, _repoCell2, _repoCell3;

        public UIImage IssueCellImage1
        {
            get { return Octicon.Gear.ToImage(); }
        }

        public UIImage IssueCellImage2
        {
            get { return Octicon.CommentDiscussion.ToImage(); }
        }

        public UIImage IssueCellImage3
        {
            get { return Octicon.Person.ToImage(); }
        }

        public UIImage IssueCellImage4
        {
            get { return Octicon.Pencil.ToImage(); }
        }

        public UIImage RepositoryCellFollowers
        {
            get { return Octicon.Star.ToImage(); }
        }

        public UIImage RepositoryCellForks
        {
            get { return Octicon.RepoForked.ToImage(); }
        }

        public UIImage RepositoryCellUser
        {
            get { return Octicon.Person.ToImage(); }
        }

        public UIColor NavigationTextColor { get { return UIColor.FromRGB(97, 95, 95); } }

        public UIColor MainTitleColor { get { return UIColor.FromRGB(0x41, 0x83, 0xc4); } }
        public UIColor MainSubtitleColor { get { return UIColor.FromRGB(81, 81, 81); } }
        public UIColor MainTextColor { get { return UIColor.FromRGB(41, 41, 41); } }

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
    }
}
