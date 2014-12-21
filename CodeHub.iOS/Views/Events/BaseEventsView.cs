using CodeHub.Core.ViewModels.Events;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Events
{
    public abstract class BaseEventsView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new EventTableViewSource(TableView, ViewModel.Events);
        }
    }
}