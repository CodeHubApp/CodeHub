using System;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.ViewComponents;
using UIKit;

namespace CodeHub.iOS.Views.Organizations
{
    public class TeamsView : BaseTableViewController<TeamsViewModel>
    {
        public TeamsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToImage(64f), "There are no teams."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RegisterClassForCellReuse(typeof(TeamCellView), TeamCellView.Key);
            var source = new ReactiveTableViewSource<TeamItemViewModel>(TableView, ViewModel.Teams, TeamCellView.Key, (float)UITableView.AutomaticDimension);
            source.ElementSelected.OfType<TeamItemViewModel>().Subscribe(x => x.GoToCommand.ExecuteIfCan());
            TableView.Source = source;
        }
    }
}