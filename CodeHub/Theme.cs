using MonoTouch.UIKit;

namespace CodeHub
{
    public class Theme : CodeFramework.ICodeFrameworkTheme
    {
        public static Theme CurrentTheme { get; private set; }

        public static void Setup()
        {
            var theme = new Theme();
            CurrentTheme = theme;
            CodeFramework.Theme.CurrentTheme = theme;
        }

        public UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/back"); } }
        public UIImage ThreeLinesButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
        public UIImage CancelButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/cancel"); } }
        public UIImage EditButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/edit"); } }
        public UIImage SaveButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        public UIImage AddButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/add"); } }
        public UIImage FilterButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/filter"); } }
        public UIImage GearButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/gear"); } }
        public UIImage ViewButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/view"); } }

        public UIImage WebBackButton { get { return UIImage.FromFile("Images/Web/back_button@2x.png"); } }
        public UIImage WebFowardButton { get { return UIImage.FromFile("Images/Web/forward_button@2x.png"); } }

        public UIImage AnonymousUserImage { get { return Images.Anonymous; } }

        public UIImage ViewBackground { get { return UIImage.FromFile("Images/Controls/background.png"); } }

        public UIImage MenuSectionBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/menu_section_bg"); } }
        public UIImage MenuNavbarBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/menu_navbar"); } }
        public UIImage WarningImage { get { return UIImageHelper.FromFileAuto("Images/Controls/warning"); } }

        public UIImage DropbarBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/dropbar"); } }

        public UIImage TableViewSectionBackground { get { return Images.Searchbar; } }

        //Cache these because we make a smaller size of them
        private UIImage _issueCell1, _issueCell2, _issueCell3, _issueCell4;
        private UIImage _repoCell1, _repoCell2, _repoCell3;

        public UIImage IssueCellImage1
        {
            get { return _issueCell1 ?? (_issueCell1 = new UIImage(Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage2
        {
            get { return _issueCell2 ?? (_issueCell2 = new UIImage(Images.CommentAdd.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage3
        {
            get { return _issueCell3 ?? (_issueCell3 = new UIImage(Images.Pencil.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage4
        {
            get { return _issueCell4 ?? (_issueCell4 = new UIImage(Images.Cog.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellFollowers
        {
            get { return _repoCell1 ?? (_repoCell1 = new UIImage(Images.Heart.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellForks
        {
            get { return _repoCell2 ?? (_repoCell2 = new UIImage(Images.Fork.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellUser
        {
            get { return _repoCell3 ?? (_repoCell3 = new UIImage(Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }
    }
}
