using System;
using ReactiveUI;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;
using System.Reactive.Linq;
using CodeFramework.iOS.Views.Source;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : FileSourceView<SourceViewModel>
    {
        private UIActionSheet _actionSheet;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => CreateActionSheet());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.SourceItem, x => x != null));

            ViewModel
                .WhenAnyValue(x => x.CurrentItem)
                .Where(x => x != null)
                .Subscribe(x => Title = x.Path.Substring(x.Path.LastIndexOf('/') + 1));

//			ViewModel.Bind(x => x.IsLoading, x =>
//			{
//				if (x) return;
//				if (!string.IsNullOrEmpty(ViewModel.ContentPath))
//				{
//					var data = System.IO.File.ReadAllText(ViewModel.ContentPath, System.Text.Encoding.UTF8);
//					LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
//				}
//				else if (!string.IsNullOrEmpty(ViewModel.FilePath))
//				{
//					LoadFile(ViewModel.FilePath);
//				}
//			});
		}

        protected void CreateActionSheet()
		{
            _actionSheet = new UIActionSheet(Title);
            var editButton = ViewModel.GoToEditCommand.CanExecute(null) ? _actionSheet.AddButton("Edit") : -1;
            var themeButton = _actionSheet.AddButton("Change Theme");
            _actionSheet.CancelButtonIndex = _actionSheet.AddButton("Cancel");
            _actionSheet.Clicked += (sender, e) =>
			{
				if (e.ButtonIndex == editButton)
                    ViewModel.GoToEditCommand.Execute(null);
                else if (e.ButtonIndex == themeButton)
                    ShowThemePicker();
                _actionSheet = null;
			};
            _actionSheet.ShowInView(View);
		}
    }
}

