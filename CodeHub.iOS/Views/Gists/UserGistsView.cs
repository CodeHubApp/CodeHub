using System;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : BaseGistsView<UserGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.IsMine)
                .Select(x => x ? ViewModel.GoToCreateGistCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }
    }
}

