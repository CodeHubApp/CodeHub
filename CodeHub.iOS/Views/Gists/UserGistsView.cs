using System;
using CodeHub.Core.ViewModels.Gists;
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

            var button = new UIBarButtonItem(UIBarButtonSystemItem.Add);

            if (ViewModel.IsMine)
                NavigationItem.RightBarButtonItem = button;

            OnActivation(d => d(button.GetClickedObservable().Subscribe(_ => GoToCreateGist())));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }

        private void GoToCreateGist()
        {
            GistCreateView.Show(this);
        }
    }
}

