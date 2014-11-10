using System;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Gists
{
    public class UserGistsView : BaseGistsView<UserGistsViewModel>
    {
        public UserGistsView()
        {
            this.WhenViewModel(x => x.IsMine)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x ? 
                    ViewModel.GoToCreateGistCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null);
        }
    }
}

