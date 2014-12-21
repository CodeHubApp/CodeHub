using CodeHub.Core.ViewModels.Releases;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleasesView : NewReactiveTableViewController<ReleasesViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new ReleasesTableViewSource(TableView, ViewModel.Releases);
        }
    }
}

