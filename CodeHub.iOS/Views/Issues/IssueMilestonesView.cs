using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : BaseTableViewController<IssueMilestonesViewModel>
    {
        public IssueMilestonesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Milestone.ToImage(64f), "There are no milestones."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueMilestoneTableViewSource(TableView, ViewModel.Milestones);
        }
    }
}

