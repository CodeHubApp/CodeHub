using System;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : WebView<ReadmeViewModel>
    {
        private UIActionSheet _actionSheet;

        public override void ViewDidLoad()
        {
            Web.ScalesPageToFit = true;
            Title = "Readme";

            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.ContentText)
                .Where(x => x != null)
                .Subscribe(x => LoadContent(new ReadmeRazorView { Model = x }.GenerateString()));

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShareButtonPress());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.ContentModel).Select(x => x != null));
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
            _actionSheet = new UIActionSheet("Readme");
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

