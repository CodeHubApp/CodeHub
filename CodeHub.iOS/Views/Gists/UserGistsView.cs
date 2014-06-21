using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : GistsView<UserGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Gists";

            base.ViewDidLoad();

            if (ViewModel.IsMine)
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => ViewModel.GoToCreateGistCommand.Execute(null));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }
    }
}

