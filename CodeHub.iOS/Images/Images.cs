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
        }

        private static UIImage CreateTemplateFromAuto(string path)
        {
            return UIImageHelper.FromFileAuto(path);
        }
    }
}

