using System;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class UserGistsViewController : BaseGistsViewController<UserGistsViewModel>
    {
        public UserGistsViewController()
        {
            this.WhenAnyValue(x => x.ViewModel.IsMine)
                .Select(x => x ? ViewModel.GoToCreateGistCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

    }
}

