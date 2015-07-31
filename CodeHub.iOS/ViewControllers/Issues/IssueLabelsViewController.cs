using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssueLabelsViewController : BaseTableViewController<IssueLabelsViewModel>
    {
        public IssueLabelsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no labels."));

            this.WhenActivated(d =>
            {
                d(this.WhenAnyValue(x => x.ViewModel.Labels)
                    .Select(x => new IssueLabelTableViewSource(TableView, x))
                    .BindTo(TableView, x => x.Source));
            });
        }

        public static void Show(UIViewController parent, IssueLabelsViewModel viewModel)
        {
            var viewController = new IssueLabelsViewController { Title = "Labels", ViewModel = viewModel };
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => {
                viewController.ViewModel.SaveCommand.ExecuteIfCan();
                viewController.DismissViewController(true, null);
            });
            parent.PresentViewController(new ThemedNavigationController(viewController), true, null);
        }
    }
}

