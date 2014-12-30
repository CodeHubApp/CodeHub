using MonoTouch.UIKit;

namespace CodeHub.iOS
{
    public static class Images
    {
        public static UIImage Merge { get { return CreateTemplateFromBundle("/Images/git-merge"); } }

        public static UIImage Globe { get { return CreateTemplateFromBundle("/Images/globe"); } }

        public static UIImage Repo { get { return CreateTemplateFromBundle("/Images/repo"); } }

        public static UIImage Organization { get { return CreateTemplateFromBundle("Images/organization"); } }

        public static UIImage Heart { get { return CreateTemplateFromBundle("/Images/heart"); } }

        public static UIImage Fork { get { return CreateTemplateFromBundle("/Images/repo-forked"); } }

        public static UIImage Pencil { get { return CreateTemplateFromBundle("/Images/pencil"); } }

        public static UIImage Tag { get { return CreateTemplateFromBundle("/Images/tag"); } }

        public static UIImage Comment { get { return CreateTemplateFromBundle("/Images/comment"); } }

        public static UIImage Trashcan { get { return CreateTemplateFromBundle("/Images/trashcan"); } }

        public static UIImage Calendar { get { return CreateTemplateFromBundle("/Images/calendar"); } }

        public static UIImage FileCode { get { return CreateTemplateFromBundle("/Images/file-code"); } }

        public static UIImage Commit { get { return CreateTemplateFromBundle("/Images/git-commit"); } }

        public static UIImage Following { get { return CreateTemplateFromBundle("/Images/following"); } }

        public static UIImage Eye { get { return CreateTemplateFromBundle("Images/eye"); } }

        public static UIImage PullRequest { get { return CreateTemplateFromBundle("Images/git-pull-request"); } }

        public static UIImage Directory { get { return CreateTemplateFromBundle("/Images/file-directory"); } }

        public static UIImage Submodule { get { return CreateTemplateFromBundle("/Images/file-submodule"); } }

        public static UIImage Branch { get { return CreateTemplateFromBundle("/Images/git-branch"); } }

        public static UIImage Question { get { return CreateTemplateFromBundle("/Images/question"); } }

        public static UIImage Rss { get { return CreateTemplateFromBundle("/Images/rss"); } }

        public static UIImage Person { get { return CreateTemplateFromBundle("/Images/person"); } }

        public static UIImage Gear { get { return CreateTemplateFromBundle("/Images/gear"); } }

        public static UIImage Star { get { return CreateTemplateFromBundle("/Images/star"); } }

        public static UIImage Alert { get { return CreateTemplateFromBundle("/Images/alert"); } }

        public static UIImage IssueOpened { get { return CreateTemplateFromBundle("/Images/issue-opened"); } }

        public static UIImage News { get { return CreateTemplateFromAuto("Images/news"); } }

        public static UIImage Inbox { get { return CreateTemplateFromAuto("Images/inbox"); } }

        public static UIImage Info { get { return CreateTemplateFromAuto("Images/info"); } }

        public static UIImage Lock { get { return CreateTemplateFromAuto("Images/lock"); } }


        public static UIImage DownChevron { get { return CreateTemplateFromAuto("Images/down_chevron"); } }

        public static UIImage RadioTower { get { return CreateTemplateFromAuto("Images/radio-tower"); } }

        public static UIImage Gist { get { return CreateTemplateFromAuto("Images/gist"); } }

        public static UIImage Code { get { return CreateTemplateFromAuto("Images/code"); } }

        public static UIImage Book { get { return CreateTemplateFromAuto("Images/book"); } }

        public static UIImage Package { get { return CreateTemplateFromAuto("Images/package"); } }

        public static UIImage Milestone { get { return CreateTemplateFromAuto("Images/milestone"); } }

        public static UIImage LightBulb { get { return CreateTemplateFromAuto("Images/light-bulb"); } }

        public static UIImage Bug { get { return CreateTemplateFromAuto("Images/bug"); } }

        public static UIImage Telescope { get { return CreateTemplateFromAuto("Images/telescope"); } }

        public static UIImage Pulse { get { return CreateTemplateFromAuto("Images/pulse"); } }

        public static UIImage Clippy { get { return CreateTemplateFromAuto("Images/clippy"); } }

        public static UIImage CheckButton { get { return CreateTemplateFromAuto("Images/Buttons/check"); } }

        public static UIImage SaveButton { get { return CreateTemplateFromAuto("Images/Buttons/save"); } }

        public static UIImage LoginUserUnknown { get { return UIImageHelper.FromFileAuto("Images/login_user_unknown"); } }

        public static UIImage Cancel { get { return UIImage.FromBundle("Images/Navigation/cancel"); } }

        public static UIImage BackButton { get { return UIImage.FromBundle("Images/Navigation/back"); } }

        public static class Logos
        {
            public static UIImage GitHub { get { return UIImage.FromFile("Images/Logos/github.png"); } }

            public static UIImage Enterprise { get { return UIImage.FromFile("Images/enterprise.png"); } }
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

