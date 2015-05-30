using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : BaseTableViewController<IssueLabelsViewModel>
    {
        public IssueLabelsView()
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

