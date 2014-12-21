using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Source
{
    public class SourceTreeView : NewReactiveTableViewController<SourceTreeViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new SourceContentTableViewSource(TableView, ViewModel.Content);
        }
    }
}

