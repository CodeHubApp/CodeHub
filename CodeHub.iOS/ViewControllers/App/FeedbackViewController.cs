using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.App
{
    public class FeedbackViewController : BaseTableViewController<FeedbackViewModel> 
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new FeedbackTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}

