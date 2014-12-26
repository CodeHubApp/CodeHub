using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.App
{
    public class FeedbackView : ReactiveTableViewController<FeedbackViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new FeedbackTableViewSource(TableView, ViewModel.Items);
        }
    }
}

