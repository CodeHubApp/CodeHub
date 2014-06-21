using MonoTouch.UIKit;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Source
{
	public abstract class FileSourceView<TViewModel> : WebView<TViewModel> where TViewModel : BaseViewModel
	{
		private bool _loaded = false;

		protected FileSourceView()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
//            NavigationItem.RightBarButtonItem.EnableIfExecutable();
//			NavigationItem.RightBarButtonItem.Enabled = false;
//			ViewModel.Bind(x => x.IsLoading, x => NavigationItem.RightBarButtonItem.Enabled = !x);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			//Stupid but I can't put this in the ViewDidLoad...
//			if (!_loaded)
//			{
//				ViewModel.LoadCommand.Execute(null);
//				_loaded = true;
//			}
//
//			Title = ViewModel.Title;
		}

		protected virtual UIActionSheet CreateActionSheet(string title)
		{
			return new UIActionSheet(title);
		}

		private void ShowExtraMenu()
		{
//			var sheet = CreateActionSheet(Title);
//			var openButton = !string.IsNullOrEmpty(ViewModel.FilePath) ? sheet.AddButton("Open In") : -1;
//			var shareButton = !string.IsNullOrEmpty(ViewModel.HtmlUrl) ? sheet.AddButton("Share") : -1;
//			var showButton = ViewModel.GoToHtmlUrlCommand.CanExecute(null) ? sheet.AddButton("Show in GitHub") : -1;
//			var cancelButton = sheet.AddButton("Cancel");
//			sheet.CancelButtonIndex = cancelButton;
//			sheet.DismissWithClickedButtonIndex(cancelButton, true);
//			sheet.Clicked += (s, e) => 
//			{
//				try
//				{
//					if (e.ButtonIndex == openButton)
//					{
//						var ctrl = new UIDocumentInteractionController();
//						ctrl.Url = NSUrl.FromFilename(ViewModel.FilePath);
//						ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
//					}
//					else if (e.ButtonIndex == shareButton)
//					{
//						ViewModel.ShareCommand.Execute(null);
//					}
//					else if (e.ButtonIndex == showButton)
//					{
//						ViewModel.GoToHtmlUrlCommand.Execute(null);
//					}
//				}
//				catch
//				{
//				}
//			};
//
//			sheet.ShowInView(this.View);
		}
    }
}

