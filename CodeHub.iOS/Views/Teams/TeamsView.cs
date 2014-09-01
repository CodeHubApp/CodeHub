using System;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using GitHubSharp.Models;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamsView : ReactiveTableViewController<TeamsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Teams";
            base.ViewDidLoad();
            this.AddSearchBar(x => ViewModel.SearchKeyword = x);
            TableView.RegisterClassForCellReuse(typeof(TeamCellView), TeamCellView.Key);
            var source = new ReactiveTableViewSource<TeamShortModel>(TableView, ViewModel.Teams, TeamCellView.Key, 44f);
            source.ElementSelected.Subscribe(ViewModel.GoToTeamCommand.ExecuteIfCan);
            TableView.Source = source;

            ViewModel.LoadCommand.ExecuteIfCan();
        }
    }
}