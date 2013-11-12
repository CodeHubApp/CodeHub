using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.ViewControllers;
using MonoTouch.UIKit;

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
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.AddButton, NewGist));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }

        private void NewGist()
        {
            var ctrl = new CreateGistViewController();
            ctrl.Created = newGist =>ViewModel.Gists.Items.Insert(0, newGist);
            NavigationController.PushViewController(ctrl, true);
        }
    }
}

