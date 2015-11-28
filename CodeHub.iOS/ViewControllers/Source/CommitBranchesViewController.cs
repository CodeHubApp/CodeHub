using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class CommitBranchesViewController : BaseTableViewController<CommitBranchesViewModel>
    {
        public CommitBranchesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no branches."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            var source = new ReactiveTableViewSource<BranchItemViewModel>(TableView, ViewModel.Items, BranchCellView.Key, 44f);
            OnActivation(d => d(source.ElementSelected.OfType<BranchItemViewModel>().Subscribe(x => x.GoToCommand.ExecuteIfCan())));
            TableView.Source = source;
        }
    }
}

