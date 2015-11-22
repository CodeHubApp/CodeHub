using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Views;
using System;
using UIKit;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssueAssigneeViewController : BaseTableViewController<IssueAssigneeViewModel>
    {
        public IssueAssigneeViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no assignees."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueAssigneeTableViewSource(TableView, ViewModel.Assignees);
        }

        public static void Show(UIViewController parent, IssueAssigneeViewModel viewModel)
        {
            var viewController = new IssueAssigneeViewController { Title = "Assignees", ViewModel = viewModel };
            parent.NavigationController.PushViewController(viewController, true);
            viewController.ViewModel.DismissCommand.Subscribe(_ => parent.NavigationController.PopToViewController(parent, true));
        }
    }
}

