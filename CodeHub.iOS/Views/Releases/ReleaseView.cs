using System;
using Xamarin.Utilities.ViewControllers;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.Releases;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleaseView : WebView<ReleaseViewModel>
    {
        private UIActionSheet _actionSheet;

        public override void ViewDidLoad()
        {
            Web.ScalesPageToFit = true;

            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.ContentText)
                .Where(x => x != null)
                .Subscribe(x => Load());

            ViewModel.WhenAnyValue(x => x.ReleaseModel)
                .Subscribe(x => Title = x == null ? "Release" : x.Name);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShareButtonPress());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
        }

        private void Load()
        {
            var model = new ReleaseRazorViewModel { Body = ViewModel.ContentText, Release = ViewModel.ReleaseModel };
            LoadContent(new ReleaseRazorView { Model = model }.GenerateString());
        }

        protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
                return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToLinkCommand.Execute(request.Url.AbsoluteString);
            return false;
        }

        private void ShareButtonPress()
        {
            _actionSheet = new UIActionSheet(Title);
            var shareButton = ViewModel.ShareCommand.CanExecute(null) ? _actionSheet.AddButton("Share") : -1;
            var showButton = ViewModel.GoToGitHubCommand.CanExecute(null) ? _actionSheet.AddButton("Show in GitHub") : -1;
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == showButton)
                    ViewModel.GoToGitHubCommand.ExecuteIfCan();
                else if (e.ButtonIndex == shareButton)
                    ViewModel.ShareCommand.ExecuteIfCan();
                _actionSheet = null;
            };

            _actionSheet.ShowInView(View);
        }
    }
}

