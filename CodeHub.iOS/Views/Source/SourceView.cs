using System;
using ReactiveUI;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : FileSourceView<SourceViewModel>
    {
        private UIActionSheet _actionSheet;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => CreateActionSheet());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.SourceItem).Select(x => x != null));
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

