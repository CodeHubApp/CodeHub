using CodeHub.Core.ViewModels.Activity;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Activity
{
    public abstract class BaseEventsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        protected BaseEventsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RadioTower.ToEmptyListImage(), "There is no activity."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new EventTableViewSource(TableView, ViewModel.Items);
        }
    }
}