using System;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
    public class CommitBranchsView : BaseTableViewController<CommitBranchesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            var source = new ReactiveTableViewSource<BranchItemViewModel>(TableView, ViewModel.Branches, BranchCellView.Key, 44f);
            source.ElementSelected.OfType<BranchItemViewModel>().Subscribe(x => x.GoToCommand.ExecuteIfCan());
            TableView.Source = source;
        }
    }
}

