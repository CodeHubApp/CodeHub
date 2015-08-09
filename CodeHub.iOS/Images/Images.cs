using UIKit;

namespace CodeHub.iOS
{
    public static class Images
    {
        public static UIImage DownChevron { get { return CreateTemplateFromAuto("Images/down_chevron"); } }

        public static UIImage SaveButton { get { return CreateTemplateFromAuto("Images/Buttons/save"); } }

        public static UIImage LoginUserUnknown { get { return UIImageHelper.FromFileAuto("Images/login_user_unknown"); } }

        public static UIImage UnknownUser { get { return UIImage.FromBundle("Images/unknown_user"); } }

        public static UIImage Cancel { get { return UIImage.FromBundle("Images/Navigation/cancel"); } }

        public static UIImage BackButton { get { return UIImage.FromBundle("Images/Navigation/back"); } }

        public static UIImage BackChevron { get { return UIImageHelper.FromFileAuto("Images/back-chevron"); } }

        public static UIImage ForwardChevron { get { return UIImageHelper.FromFileAuto("Images/forward-chevron"); } }

        public static UIImage Search { get { return UIImageHelper.FromFileAuto("Images/search"); } }

        public static UIImage Sort { get { return UIImageHelper.FromFileAuto("Images/sort"); } }

        public static UIImage Filter { get { return UIImageHelper.FromFileAuto("Images/filter"); } }

        public static UIImage FilterFilled { get { return UIImageHelper.FromFileAuto("Images/filter-filled"); } }

        public static class Logos
        {
            public static UIImage GitHub { get { return UIImage.FromFile("Images/dotcom-mascot.png"); } }

            public static UIImage Enterprise { get { return UIImage.FromFile("Images/enterprise-mascot.png"); } }
        }

        public static class Buttons
        {
            public static UIImage BlackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/black_button"); } }

            public static UIImage GreyButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/grey_button"); } }
        }


        private static UIImage CreateTemplateFromBundle(string path)
        {
            return UIImage.FromBundle(path);
        }

        private static UIImage CreateTemplateFromAuto(string path)
        {
            return UIImageHelper.FromFileAuto(path);
        }
    }
}

