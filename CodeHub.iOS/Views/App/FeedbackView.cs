using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

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

