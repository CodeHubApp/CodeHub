using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class BaseGistsView<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : BaseGistsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new GistTableViewSource(TableView, ViewModel.Gists);
        }
    }
}

