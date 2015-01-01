using System;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.iOS.Views.Organizations
{
    public class TeamsView : BaseTableViewController<TeamsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RegisterClassForCellReuse(typeof(TeamCellView), TeamCellView.Key);
            var source = new ReactiveTableViewSource<TeamItemViewModel>(TableView, ViewModel.Teams, TeamCellView.Key, 44f);
            source.ElementSelected.OfType<TeamItemViewModel>().Subscribe(x => x.GoToCommand.ExecuteIfCan());
            TableView.Source = source;
        }
    }
}