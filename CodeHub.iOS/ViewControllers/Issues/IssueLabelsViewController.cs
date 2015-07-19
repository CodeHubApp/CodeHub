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
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Labels)
                .Select(x => new IssueLabelTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}

