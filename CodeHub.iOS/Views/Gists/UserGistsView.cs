using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.ViewControllers;
using UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : GistsView
    {
        public new UserGistsViewModel ViewModel
        {
            get { return (UserGistsViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

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

