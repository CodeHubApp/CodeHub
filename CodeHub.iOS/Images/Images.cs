using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS
{
    public static class Images
    {
        public static UIImage FromBundle(string path)
        {
            var img = UIImage.FromBundle(path);
            var template = img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return template;
        }

        public static UIImage FromFileAuto(string path)
        {
            var img = UIImageHelper.FromFileAuto(path);
            var template = img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return template;
        }

        public static UIImage Merge { get { return FromBundle("/Images/merge"); } }
        public static UIImage Language { get { return FromBundle("/Images/language"); } }
        public static UIImage Webpage { get { return FromBundle("/Images/webpage"); } }
        public static UIImage Repo { get { return FromBundle("/Images/repo"); } }
        public static UIImage Size { get { return FromBundle("/Images/size"); } }
        public static UIImage Locked { get { return FromBundle("/Images/locked"); } }
        public static UIImage Unlocked { get { return FromBundle("/Images/unlocked"); } }
        public static UIImage Heart { get { return FromBundle("/Images/heart"); } }
        public static UIImage Fork { get { return FromBundle("/Images/fork"); } }
        public static UIImage Pencil { get { return FromBundle("/Images/pencil"); } }
        public static UIImage Tag { get { return FromBundle("/Images/tag"); } }
        public static UIImage Comments { get { return FromBundle("/Images/comments"); } }
        public static UIImage BinClosed { get { return FromBundle("/Images/bin_closed"); } }
        public static UIImage Milestone { get { return FromBundle("/Images/milestone"); } }
        public static UIImage Script { get { return FromBundle("/Images/script"); } }
        public static UIImage Commit { get { return FromBundle("/Images/commit"); } }
        public static UIImage Following { get { return FromBundle("/Images/following"); } }
        public static UIImage Folder { get { return FromBundle("/Images/folder"); } }
        public static UIImage File { get { return FromBundle("/Images/file"); } }
        public static UIImage Branch { get { return FromBundle("/Images/branch"); } }
        public static UIImage Create { get { return FromBundle("/Images/create"); } }

        public static UIImage Info { get { return FromBundle("/Images/info"); } }

        public static UIColor IssueColor { get { return UIColor.FromRGB(0x6c, 0xc6, 0x44); } }
        public static UIImage Issue { get { return FromBundle("/Images/flag"); } }

        public static UIImage User { get { return FromBundle("/Images/user"); } }
        public static UIImage Explore { get { return FromBundle("/Images/explore"); } }
        public static UIImage Group { get { return FromBundle("/Images/group"); } }
        public static UIImage Event { get { return FromBundle("/Images/events"); } }
        public static UIImage Cog { get { return FromBundle("/Images/cog"); } }
        public static UIImage Star { get { return FromBundle("/Images/star"); } }
        public static UIImage News { get { return FromBundle("/Images/news"); } }
        public static UIImage Notifications { get { return FromBundle("/Images/notifications"); } }
        public static UIImage Priority { get { return FromBundle("/Images/priority"); } }
        public static UIImage Anonymous { get { return FromBundle("/Images/anonymous"); } }

        public static UIImage Team { get { return FromFileAuto("Images/team"); } }
        public static UIImage Eye { get { return FromFileAuto("Images/eye"); } }
        public static UIImage Hand { get { return FromFileAuto("Images/hand"); } }
        public static UIImage Bug { get { return FromFileAuto("Images/bug"); } }


        public static UIImage MenuBackground { get { return UIImageHelper.FromFileAuto("Images/codehub-blur"); } }


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

        public static class Gist
        {
            public static UIImage Share { get { return UIImageHelper.FromFileAuto("Images/Gist/share"); } }
            public static UIImage Star { get { return UIImageHelper.FromFileAuto("Images/Gist/star"); } }
            public static UIImage StarHighlighted { get { return UIImageHelper.FromFileAuto("Images/Gist/star_highlighted"); } }
            public static UIImage User { get { return UIImageHelper.FromFileAuto("Images/Gist/user"); } }
        }

//        public static class Notifications
//        {
//            public static UIImage Commit { get { return UIImageHelper.FromFileAuto("Images/Notifications/commit"); } }
//            public static UIImage PullRequest { get { return UIImageHelper.FromFileAuto("Images/Notifications/pull_request"); } }
//        }
    }
}

