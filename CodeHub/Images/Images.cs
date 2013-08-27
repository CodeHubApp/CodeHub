using MonoTouch.UIKit;

namespace CodeHub
{
    public static class Images
    {
        public static UIImage ScmType { get { return UIImage.FromBundle("/Images/scm_type"); } }
        public static UIImage Language { get { return UIImage.FromBundle("/Images/language"); } }
        public static UIImage Webpage { get { return UIImage.FromBundle("/Images/webpage"); } }
        public static UIImage Repo { get { return UIImage.FromBundle("/Images/repo"); } }

        public static UIImage RepoFollow { get { return UIImageHelper.FromFileAuto("Images/repo_follow"); } }
        public static UIImage Team { get { return UIImageHelper.FromFileAuto("Images/team"); } }


        public static UIImage Size { get { return UIImage.FromBundle("/Images/size"); } }
        public static UIImage Locked { get { return UIImage.FromBundle("/Images/locked"); } }
        public static UIImage Unlocked { get { return UIImage.FromBundle("/Images/unlocked"); } }
        public static UIImage Heart { get { return UIImage.FromBundle("/Images/heart"); } }
        public static UIImage HeartAdd { get { return UIImage.FromBundle("/Images/heart_add"); } }
        public static UIImage HeartDelete { get { return UIImage.FromBundle("/Images/heart_delete"); } }
        public static UIImage Fork { get { return UIImage.FromBundle("/Images/fork"); } }
        public static UIImage Pencil { get { return UIImage.FromBundle("/Images/pencil"); } }
        public static UIImage Plus { get { return UIImage.FromBundle("/Images/plus"); } }
        public static UIImage Tag { get { return UIImage.FromBundle("/Images/tag"); } }
        public static UIImage CommentAdd { get { return UIImage.FromBundle("/Images/comment_add"); } }
        public static UIImage ReportEdit { get { return UIImage.FromBundle("/Images/report_edit"); } }
        public static UIImage BinClosed { get { return UIImage.FromBundle("/Images/bin_closed"); } }
        public static UIImage Milestone { get { return UIImage.FromBundle("/Images/milestone"); } }
        public static UIImage ServerComponents { get { return UIImage.FromBundle("/Images/server_components"); } }
        public static UIImage SitemapColor { get { return UIImage.FromBundle("/Images/sitemap_color"); } }
        public static UIImage Script { get { return UIImage.FromBundle("/Images/script"); } }


        public static UIImage Folder { get { return UIImage.FromBundle("/Images/folder"); } }
        public static UIImage File { get { return UIImage.FromBundle("/Images/file"); } }
        public static UIImage Branch { get { return UIImage.FromBundle("/Images/branch"); } }
        public static UIImage Create { get { return UIImage.FromBundle("/Images/create"); } }

        public static UIImage Changes { get { return UIImage.FromBundle("/Images/changes"); } }



        public static UIImage BackButton { get { return UIImage.FromBundle("/Images/Controls/backbutton"); } }
        public static UIImage BackButtonLandscape { get { return UIImage.FromBundle("/Images/Controls/backbutton-landscape"); } }

        public static UIImage BarButton { get { return UIImage.FromBundle("/Images/Controls/barbutton"); } }
        public static UIImage BarButtonLandscape { get { return UIImage.FromBundle("/Images/Controls/barbutton-land"); } }

        public static UIImage Titlebar { get { return UIImage.FromBundle("/Images/Controls/titlebar"); } }
        public static UIImage Bottombar { get { return UIImage.FromFile("Images/Controls/bottombar.png"); } }
        public static UIImage Searchbar { get { return UIImage.FromBundle("/Images/Controls/searchbar"); } }
        public static UIImage Divider { get { return UIImage.FromBundle("/Images/Controls/divider"); } }

        public static UIImage TableCell { get { return UIImage.FromBundle("/Images/TableCell"); } }
        public static UIImage TableCellRed { get { return UIImage.FromBundle("/Images/tablecell_red"); } }

        //Issues
        public static UIImage Priority { get { return UIImage.FromBundle("/Images/priority"); } }
        public static UIImage Anonymous { get { return UIImage.FromBundle("/Images/anonymous"); } }


        //Size agnostic
        public static UIImage Background { get { return UIImage.FromFile("Images/Controls/background.png"); } }
        public static UIImage Dropbar { get { return UIImageHelper.FromFileAuto("Images/Controls/dropbar"); } }

        public static System.Uri GitHubRepoUrl
        {
            get { return new System.Uri(System.IO.Path.Combine(MonoTouch.Foundation.NSBundle.MainBundle.ResourcePath, "Images/repository.png")); }
        }

        public static System.Uri GitHubRepoForkUrl
        {
            get { return new System.Uri(System.IO.Path.Combine(MonoTouch.Foundation.NSBundle.MainBundle.ResourcePath, "Images/repository_fork.png")); }
        }
		
        public static class Logos
        {
            public static UIImage GitHub { get { return UIImage.FromFile("Images/Logos/github.png"); } }
        }

        public static class Buttons
        {
            public static UIImage Info { get { return UIImage.FromBundle("/Images/Buttons/info"); } }
            public static UIImage Flag { get { return UIImage.FromBundle("/Images/Buttons/flag"); } }
            public static UIImage User { get { return UIImage.FromBundle("/Images/Buttons/user"); } }
            public static UIImage Explore { get { return UIImage.FromBundle("/Images/Buttons/explore"); } }
            public static UIImage Group { get { return UIImage.FromBundle("/Images/Buttons/group"); } }
            public static UIImage Event { get { return UIImage.FromBundle("/Images/Buttons/events"); } }
            public static UIImage Person { get { return UIImage.FromBundle("/Images/Buttons/person"); } }
            public static UIImage Cog { get { return UIImage.FromBundle("/Images/Buttons/cog"); } }
        }
    }
}

