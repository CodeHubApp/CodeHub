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
            var obs = this.WhenAnyValue(x => x.ViewModel.IsMine).Where(x => x).Select(_ => ViewModel.GoToCreateGistCommand);
            OnActivation(d => d(obs.ToBarButtonItem(UIBarButtonSystemItem.Add, x => NavigationItem.RightBarButtonItem = x)));
        }
    }
}

