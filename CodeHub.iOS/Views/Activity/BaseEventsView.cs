using CodeHub.Core.ViewModels.Events;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Activity
{
    public abstract class BaseEventsView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new EventTableViewSource(TableView, ViewModel.Events);
        }
    }
}