using System;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using GitHubSharp.Models;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetBranchesView : ReactiveTableViewController<ChangesetBranchesViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Changeset Branch";

            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            var source = new ReactiveTableViewSource<BranchModel>(TableView, ViewModel.Branches, BranchCellView.Key, 44f);
            source.ElementSelected.Subscribe(ViewModel.GoToBranchCommand.ExecuteIfCan);
            TableView.Source = source;

            ViewModel.LoadCommand.ExecuteIfCan();
        }
    }
}

