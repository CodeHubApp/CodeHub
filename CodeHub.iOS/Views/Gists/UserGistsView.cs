using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : GistsView<UserGistsViewModel>
    {
        public UserGistsView()
            : base(true)
        {
            Title = "Gists";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (ViewModel.IsMine)
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add).WithCommand(ViewModel.GoToCreateGistCommand);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }
    }
}

