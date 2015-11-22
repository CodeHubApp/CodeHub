using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssueLabelsViewController : BaseTableViewController<IssueLabelsViewModel>
    {
        public IssueLabelsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no labels."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueLabelTableViewSource(TableView, ViewModel.Labels);
        }

        public static void Show(UIViewController parent, IssueLabelsViewModel viewModel)
        {
            var viewController = new IssueLabelsViewController { Title = "Labels", ViewModel = viewModel };
            var backButton = new UIBarButtonItem { Image = Images.BackButton };
            viewController.NavigationItem.LeftBarButtonItem = backButton;
            viewController.WhenActivated(d => d(backButton.GetClickedObservable().Subscribe(_ => {
                viewController.ViewModel.SaveCommand.ExecuteIfCan();
                viewController.NavigationController.PopToViewController(parent, true);
            })));
            parent.NavigationController.PushViewController(viewController, true);
        }
    }
}

