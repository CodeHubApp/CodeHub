using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.App
{
    public class FeedbackViewController : BaseTableViewController<FeedbackViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.WhenAnyValue(x => x.ViewModel.Items)
                .BindTableSource(TableView, (tv, x) => new FeedbackTableViewSource(tv, x));
        }
    }
}

