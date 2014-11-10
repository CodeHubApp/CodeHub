using CodeHub.Core.ViewModels.Releases;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleasesView : ReactiveTableViewController<ReleasesViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new ReleasesTableViewSource(TableView, ViewModel.Releases);
        }
    }
}

