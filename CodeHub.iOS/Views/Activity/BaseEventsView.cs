using System;
using CodeHub.Core.ViewModels.Activity;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using UIKit;

namespace CodeHub.iOS.Views.Activity
{
    public abstract class BaseEventsView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        protected BaseEventsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RadioTower.ToImage(64f), "There is no activity."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new EventTableViewSource(TableView, ViewModel.Events);
        }
    }
}