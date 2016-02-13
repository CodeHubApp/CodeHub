using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using Foundation;
using CodeHub.iOS.Services;

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
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.ViewButton, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.GoToFileSourceCommand.Execute(null));
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

        protected override void OnLoadError(NSError error)
        {
            base.OnLoadError(error);
            AlertDialogService.ShowAlert("Error", "Unable to display this type of file.");
        }
    }
}