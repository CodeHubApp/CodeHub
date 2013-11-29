using System;
using CodeFramework.iOS.Views;
using MonoTouch.UIKit;
using CodeFramework.Core.ViewModels;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Source
{
	public abstract class FileSourceView : WebView
    {
		private bool _loaded = false;

		public new FileSourceViewModel ViewModel
		{ 
			get { return (FileSourceViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public FileSourceView()
			: base(false)
		{
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			//Stupid but I can't put this in the ViewDidLoad...
			if (!_loaded)
			{
				ViewModel.LoadCommand.Execute(null);
				_loaded = true;
			}

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
					ViewModel.GoToHtmlUrlCommand.Execute(null);
				}
			};

			sheet.ShowInView(this.View);
		}
    }
}

