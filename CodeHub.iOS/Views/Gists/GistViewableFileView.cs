using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class GistViewableFileView : WebView
    {
        public new GistViewableFileViewModel ViewModel
        {
            get { return (GistViewableFileViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public GistViewableFileView()
                : base(true)
        {
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.ViewButton, () => ViewModel.GoToFileSourceCommand.Execute(null)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Bind(x => x.FilePath, path => LoadFile(path));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null)
                Title = ViewModel.GistFile.Filename;
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            base.OnLoadError(sender, e);
            MonoTouch.Utilities.ShowAlert("Error", "Unable to display this type of file.");
        }
    }
}