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
            OnActivation(d => 
                d(this.WhenAnyValue(x => x.ViewModel.IsMine)
                .Select(_ => ViewModel.GoToCreateGistCommand)
                .ToBarButtonItem(UIBarButtonSystemItem.Add, x => NavigationItem.RightBarButtonItem = x)));
        }
    }
}

