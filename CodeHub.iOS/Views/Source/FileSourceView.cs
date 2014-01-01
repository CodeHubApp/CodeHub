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

		protected FileSourceView()
			: base(false)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
			NavigationItem.RightBarButtonItem.Enabled = false;
			ViewModel.Bind(x => x.IsLoading, x => NavigationItem.RightBarButtonItem.Enabled = !x);
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
			var openButton = !string.IsNullOrEmpty(ViewModel.FilePath) ? sheet.AddButton("Open In".t()) : -1;
			var shareButton = !string.IsNullOrEmpty(ViewModel.HtmlUrl) ? sheet.AddButton("Share".t()) : -1;
			var showButton = ViewModel.GoToHtmlUrlCommand.CanExecute(null) ? sheet.AddButton("Show in GitHub".t()) : -1;
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Clicked += (s, e) => 
			{
				try
				{
					if (e.ButtonIndex == openButton)
					{
						var ctrl = new UIDocumentInteractionController();
						ctrl.Url = NSUrl.FromFilename(ViewModel.FilePath);
						ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
					}
					else if (e.ButtonIndex == shareButton)
					{
						ViewModel.ShareCommand.Execute(null);
					}
					else if (e.ButtonIndex == showButton)
					{
						ViewModel.GoToHtmlUrlCommand.Execute(null);
					}
				}
				catch (Exception ex)
				{
				}
			};

			sheet.ShowInView(this.View);
		}
    }
}

