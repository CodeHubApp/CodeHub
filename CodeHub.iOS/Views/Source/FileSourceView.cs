using System;
using CodeHub.iOS.Views;
using UIKit;
using CodeHub.Core.ViewModels;
using Foundation;

namespace CodeHub.iOS.Views.Source
{
	public abstract class FileSourceView : WebView
    {
        private readonly UIBarButtonItem _actionButton;
		private bool _loaded = false;

		public new FileSourceViewModel ViewModel
		{ 
			get { return (FileSourceViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected FileSourceView()
			: base(false)
		{
            _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu()) { Enabled = false };
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            ViewModel.Bind(x => x.IsLoading, x => _actionButton.Enabled = !x);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

            NavigationItem.RightBarButtonItem = _actionButton;

			//Stupid but I can't put this in the ViewDidLoad...
			if (!_loaded)
			{
				ViewModel.LoadCommand.Execute(null);
				_loaded = true;
			}

			Title = ViewModel.Title;
		}

        public override void ViewDidDisappear(bool animated)
        {
            NavigationItem.RightBarButtonItem = null;
            base.ViewDidDisappear(animated);
        }

		protected virtual UIActionSheet CreateActionSheet(string title)
		{
            return new UIActionSheet();
		}

		private void ShowExtraMenu()
		{
			var sheet = CreateActionSheet(Title);
            var vm = ViewModel;
			var openButton = !string.IsNullOrEmpty(ViewModel.FilePath) ? sheet.AddButton("Open In".t()) : -1;
			var shareButton = !string.IsNullOrEmpty(ViewModel.HtmlUrl) ? sheet.AddButton("Share".t()) : -1;
			var showButton = ViewModel.GoToHtmlUrlCommand.CanExecute(null) ? sheet.AddButton("Show in GitHub".t()) : -1;
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Dismissed += (s, e) => 
			{
				BeginInvokeOnMainThread(() =>
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
                            vm.ShareCommand.Execute(null);
						}
						else if (e.ButtonIndex == showButton)
						{
                            vm.GoToHtmlUrlCommand.Execute(null);
						}
					}
					catch
					{
					}
				});

                sheet.Dispose();
			};
            sheet.ShowFrom(_actionButton, true);
		}
    }
}

