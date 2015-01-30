using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : BaseTableViewController<IssueLabelsViewModel>
    {
        public IssueLabelsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToImage(64f), "There are no labels."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueLabelTableViewSource(TableView, ViewModel.Labels);
        }
    }
}

