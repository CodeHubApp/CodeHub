using CodeHub.Core.ViewModels.Repositories;
using CodeFramework.iOS.Views;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : WebView
    {
        public new ReadmeViewModel ViewModel
        {
            get { return (ReadmeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

		public ReadmeView() : base(false)
        {
            Title = "Readme";
            Web.ScalesPageToFit = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Bind(x => x.Path, x => LoadFile(x));
			NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem(UIKit.UIBarButtonSystemItem.Action, (s, e) => ShareButtonPress());
			ViewModel.LoadCommand.Execute(false);
        }

		protected override bool ShouldStartLoad(Foundation.NSUrlRequest request, UIKit.UIWebViewNavigationType navigationType)
		{
			if (!request.Url.AbsoluteString.StartsWith("file://", System.StringComparison.Ordinal))
			{
				ViewModel.GoToLinkCommand.Execute(request.Url.AbsoluteString);
				return false;
			}

			return base.ShouldStartLoad(request, navigationType);
		}

		private void ShareButtonPress()
		{
			var sheet = MonoTouch.Utilities.GetSheet("Readme");
			var shareButton = sheet.AddButton("Share".t());
			var showButton = sheet.AddButton("Show in GitHub".t());
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);


			sheet.Dismissed += (sender, e) => {
				BeginInvokeOnMainThread(() =>
					{
						if (e.ButtonIndex == showButton)
							ViewModel.GoToGitHubCommand.Execute(null);
						else if (e.ButtonIndex == shareButton)
							ViewModel.ShareCommand.Execute(null);
					});
			};

			sheet.ShowFrom(NavigationItem.RightBarButtonItem, true);
		}
    }
}

