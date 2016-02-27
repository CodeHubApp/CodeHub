using UIKit;
using MonoTouch.UIKit;

namespace CodeHub.iOS
{
    public static class Images
    {
        public static UIImage LoginUserUnknown { get { return UIImageHelper.FromFileAuto("Images/login_user_unknown").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate); } }
        public static UIImage Avatar { get { return UIImageHelper.FromFileAuto("Images/avatar"); } }
        public static UIImage DownChevron { get { return CreateTemplateFromAuto("Images/down_chevron"); } }

        public static class Logos
        {
            public static UIImage DotComMascot { get { return UIImage.FromFile("Images/Logos/dotcom-mascot.png"); } }
            public static UIImage EnterpriseMascot { get { return UIImage.FromFile("Images/Logos/enterprise-mascot.png"); } }
        }

        public static class Buttons
        {
            public static UIImage BlackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/black_button"); } }
            public static UIImage GreyButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/grey_button"); } }
            public static UIImage CheckButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/check"); } }
            public static UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/back"); } }
            public static UIImage ThreeLinesButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
            public static UIImage CancelButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/cancel"); } }
            public static UIImage SortButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/sort"); } }
            public static UIImage SaveButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        }

        public static class Web
        {
            public static UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Web/back"); } }
            public static UIImage FowardButton { get { return UIImageHelper.FromFileAuto("Images/Web/forward"); } }
        }

        private static UIImage CreateTemplateFromAuto(string path)
        {
            return UIImageHelper.FromFileAuto(path);
        }
    }
}

