using System;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : BaseGistsView<UserGistsViewModel>
    {
        public UserGistsView()
        {
            this.WhenAnyValue(x => x.ViewModel.IsMine)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x ? 
                    ViewModel.GoToCreateGistCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null);
        }
    }
}

