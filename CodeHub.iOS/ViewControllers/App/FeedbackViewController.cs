using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.ViewControllers.App
{
    public class FeedbackViewController : BaseTableViewController<FeedbackViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new FeedbackTableViewSource(TableView, ViewModel.Items);
        }
    }
}

