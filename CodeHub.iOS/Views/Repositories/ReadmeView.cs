using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Views;
using UIKit;
using WebKit;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : WebView
    {
        private readonly UIBarButtonItem _actionButton;

        public new ReadmeViewModel ViewModel
        {
            get { return (ReadmeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

		public ReadmeView() : base(false)
        {
            Title = "Readme";
            _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShareButtonPress());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Bind(x => x.Path, x => LoadFile(x));
			ViewModel.LoadCommand.Execute(false);
        }

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
		{
            
            if (!navigationAction.Request.Url.AbsoluteString.StartsWith("file://", System.StringComparison.Ordinal))
			{
                ViewModel.GoToLinkCommand.Execute(navigationAction.Request.Url.AbsoluteString);
				return false;
			}

            return base.ShouldStartLoad(webView, navigationAction);
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationItem.RightBarButtonItem = _actionButton;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

		private void ShareButtonPress()
		{
            var sheet = new UIActionSheet();
			var shareButton = sheet.AddButton("Share".t());
			var showButton = sheet.AddButton("Show in GitHub".t());
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);

            sheet.Dismissed += (sender, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == showButton)
                        ViewModel.GoToGitHubCommand.Execute(null);
                    else if (e.ButtonIndex == shareButton)
                        ViewModel.ShareCommand.Execute(null);
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(_actionButton, true);
		}
    }
}

