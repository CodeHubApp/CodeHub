using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class BaseGistsView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : BaseGistsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new GistTableViewSource(TableView, ViewModel.Gists);
        }
    }
}

