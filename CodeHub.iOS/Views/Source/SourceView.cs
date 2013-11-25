using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : WebView
    {
		public new SourceViewModel ViewModel
		{
			get { return (SourceViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

        public SourceView()
			: base(false)
        {
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewModel.Bind(x => x.IsLoading, x =>
			{
					if (x) return;
					if (!string.IsNullOrEmpty(ViewModel.ContentPath))
					{
						var data = System.IO.File.ReadAllText(ViewModel.ContentPath, System.Text.Encoding.UTF8);
						LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
					}
					else if (!string.IsNullOrEmpty(ViewModel.FilePath))
					{
						LoadFile(ViewModel.FilePath);
					}
			});
			ViewModel.LoadCommand.Execute(null);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			Title = ViewModel.Title;
		}

		private void ShowExtraMenu()
		{
			var sheet = MonoTouch.Utilities.GetSheet(Title);

			var openButton = sheet.AddButton("Open In".t());
			var shareButton = sheet.AddButton("Share".t());
			var showButton = ViewModel.HtmlUrl != null ? sheet.AddButton("Show in GitHub".t()) : -1;
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Clicked += (s, e) => {
				if (e.ButtonIndex == openButton)
				{
					var ctrl = new UIDocumentInteractionController();
					ctrl.Url = NSUrl.FromFilename(ViewModel.FilePath);
					ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
				}
				else if (e.ButtonIndex == shareButton)
				{
					var item = UIActivity.FromObject (ViewModel.HtmlUrl);
					var activityItems = new NSObject[] { item };
					UIActivity[] applicationActivities = null;
					var activityController = new UIActivityViewController (activityItems, applicationActivities);
					PresentViewController (activityController, true, null);
				}
				else if (e.ButtonIndex == showButton)
				{
					ViewModel.GoToGitHubCommand.Execute(null);
				}
			};

			sheet.ShowInView(this.View);
		}
    }
}

