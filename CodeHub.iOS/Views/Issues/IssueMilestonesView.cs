using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : BaseTableViewController<IssueMilestonesViewModel>
    {
        public IssueMilestonesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Milestone.ToEmptyListImage(), "There are no milestones."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Milestones)
                .Select(x => new IssueMilestoneTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}

