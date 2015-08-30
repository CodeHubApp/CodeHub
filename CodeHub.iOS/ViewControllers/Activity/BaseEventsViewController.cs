using CodeHub.Core.ViewModels.Activity;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;
using UIKit;
using System;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Activity
{
    public abstract class BaseEventsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RadioTower.ToEmptyListImage(), "There is no activity."));

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new EventTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}