using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : BaseGistsView<UserGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (ViewModel.IsMine)
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add).WithCommand(ViewModel.GoToCreateGistCommand);
            ViewModel.LoadCommand.ExecuteIfCan();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }
    }
}

