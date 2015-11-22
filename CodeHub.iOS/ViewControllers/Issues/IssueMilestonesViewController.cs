using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Views;
using UIKit;
using System;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssueMilestonesViewController : BaseTableViewController<IssueMilestonesViewModel>
    {
        public IssueMilestonesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Milestone.ToEmptyListImage(), "There are no milestones."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueMilestoneTableViewSource(TableView, ViewModel.Milestones);
        }

        public static void Show(UIViewController parent, IssueMilestonesViewModel viewModel)
        {
            var viewController = new IssueMilestonesViewController { Title = "Milestones", ViewModel = viewModel };
            parent.NavigationController.PushViewController(viewController, true);
            viewController.ViewModel.DismissCommand.Subscribe(_ => parent.NavigationController.PopToViewController(parent, true));
        }
    }
}

