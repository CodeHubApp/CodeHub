using System;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamsView : NewReactiveTableViewController<TeamsViewModel>
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