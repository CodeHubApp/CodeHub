using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub
{
    public static class Images
    {
        public static UIImage Brick { get { return UIImage.FromBundle("/Images/brick"); } }
        public static UIImage Language { get { return UIImage.FromBundle("/Images/language"); } }
        public static UIImage Webpage { get { return UIImage.FromBundle("/Images/webpage"); } }
        public static UIImage Repo { get { return UIImage.FromBundle("/Images/repo"); } }
        public static UIImage RepoFollow { get { return UIImageHelper.FromFileAuto("Images/repo_follow"); } }
        public static UIImage Team { get { return UIImageHelper.FromFileAuto("Images/team"); } }
        public static UIImage Size { get { return UIImage.FromBundle("/Images/size"); } }
        public static UIImage Locked { get { return UIImage.FromBundle("/Images/locked"); } }
        public static UIImage Unlocked { get { return UIImage.FromBundle("/Images/unlocked"); } }
        public static UIImage Heart { get { return UIImage.FromBundle("/Images/heart"); } }
        public static UIImage Fork { get { return UIImage.FromBundle("/Images/fork"); } }
        public static UIImage Pencil { get { return UIImage.FromBundle("/Images/pencil"); } }
        public static UIImage Tag { get { return UIImage.FromBundle("/Images/tag"); } }
        public static UIImage Comments { get { return UIImage.FromBundle("/Images/comments"); } }
        public static UIImage BinClosed { get { return UIImage.FromBundle("/Images/bin_closed"); } }
        public static UIImage Milestone { get { return UIImage.FromBundle("/Images/milestone"); } }
        public static UIImage Script { get { return UIImage.FromBundle("/Images/script"); } }
        public static UIImage Commit { get { return UIImage.FromBundle("/Images/commit"); } }
        public static UIImage Following { get { return UIImage.FromBundle("/Images/following"); } }
        public static UIImage Eye { get { return UIImageHelper.FromFileAuto("Images/eye"); } }
        public static UIImage Hand { get { return UIImageHelper.FromFileAuto("Images/hand"); } }
        public static UIImage Folder { get { return UIImage.FromBundle("/Images/folder"); } }
        public static UIImage File { get { return UIImage.FromBundle("/Images/file"); } }
        public static UIImage Branch { get { return UIImage.FromBundle("/Images/branch"); } }
        public static UIImage Create { get { return UIImage.FromBundle("/Images/create"); } }
        public static UIImage Info { get { return UIImage.FromBundle("/Images/info"); } }
        public static UIImage Flag { get { return UIImage.FromBundle("/Images/flag"); } }
        public static UIImage User { get { return UIImage.FromBundle("/Images/user"); } }
        public static UIImage Explore { get { return UIImage.FromBundle("/Images/explore"); } }
        public static UIImage Group { get { return UIImage.FromBundle("/Images/group"); } }
        public static UIImage Event { get { return UIImage.FromBundle("/Images/events"); } }
        public static UIImage Person { get { return UIImage.FromBundle("/Images/person"); } }
        public static UIImage Cog { get { return UIImage.FromBundle("/Images/cog"); } }
        public static UIImage Star { get { return UIImage.FromBundle("/Images/star"); } }
        public static UIImage Star2 { get { return UIImage.FromBundle("/Images/star2"); } }
        public static UIImage News { get { return UIImage.FromBundle("/Images/news"); } }
        public static UIImage Public { get { return UIImage.FromBundle("/Images/public"); } }
        public static UIImage Notifications { get { return UIImage.FromBundle("/Images/notifications"); } }
        public static UIImage Priority { get { return UIImage.FromBundle("/Images/priority"); } }
        public static UIImage Anonymous { get { return UIImage.FromBundle("/Images/anonymous"); } }

        public static UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Controls/backbutton"); } }
        public static UIImage BackButtonLandscape { get { return UIImageHelper.FromFileAuto("Images/Controls/backbutton-landscape"); } }
        public static UIImage BarButton { get { return UIImageHelper.FromFileAuto("Images/Controls/barbutton"); } }
        public static UIImage BarButtonLandscape { get { return UIImageHelper.FromFileAuto("Images/Controls/barbutton-land"); } }

        public static UIImage Titlebar { get { return UIImage.FromBundle("/Images/Controls/titlebar"); } }
        public static UIImage Bottombar { get { return UIImage.FromFile("Images/Controls/bottombar.png"); } }
        public static UIImage Searchbar { get { return UIImage.FromBundle("/Images/Controls/searchbar"); } }
        public static UIImage Divider { get { return UIImage.FromBundle("/Images/Controls/divider"); } }

        public static UIImage TableCell { get { return UIImage.FromBundle("/Images/TableCell"); } }
        public static UIImage TableCellRed { get { return UIImageHelper.FromFileAuto("Images/tablecell_red"); } }

        public static Uri GitHubRepoUrl
        {
            get { return new Uri(Path.Combine(NSBundle.MainBundle.ResourcePath, "Images/repository.png")); }
        }

        public static Uri GitHubRepoForkUrl
        {
            get { return new Uri(Path.Combine(NSBundle.MainBundle.ResourcePath, "Images/repository_fork.png")); }
        }
		
        public static class Logos
        {
            public static UIImage GitHub { get { return UIImage.FromFile("Images/Logos/github.png"); } }
        }

        public static class Buttons
        {
            public static UIImage BlackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/black_button"); } }
            public static UIImage GreyButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/grey_button"); } }
        }

//        public static class Notifications
//        {
//            public static UIImage Commit { get { return UIImageHelper.FromFileAuto("Images/Notifications/commit"); } }
//            public static UIImage PullRequest { get { return UIImageHelper.FromFileAuto("Images/Notifications/pull_request"); } }
//        }
    }
}

