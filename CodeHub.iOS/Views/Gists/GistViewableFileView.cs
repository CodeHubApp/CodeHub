using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using Foundation;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Gists
{
    public class GistViewableFileView : WebView
    {
        private readonly UIBarButtonItem _viewButton = new UIBarButtonItem { Image = Theme.CurrentTheme.ViewButton };

        public new GistViewableFileViewModel ViewModel
        {
            get { return (GistViewableFileViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public GistViewableFileView()
                : base(true)
        {
            NavigationItem.RightBarButtonItem = _viewButton;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Bind(x => x.FilePath).Subscribe(x => LoadFile(x));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null)
                Title = ViewModel.GistFile.Filename;
            _viewButton.Clicked += ViewButtonClick;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _viewButton.Clicked -= ViewButtonClick;
        }

        void ViewButtonClick (object sender, System.EventArgs e)
        {
            ViewModel.GoToFileSourceCommand.Execute(null);
        }

        protected override void OnLoadError(NSError error)
        {
            base.OnLoadError(error);
            AlertDialogService.ShowAlert("Error", "Unable to display this type of file.");
        }
    }
}