using System;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class TeamsViewController : BaseTableViewController<TeamsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RegisterClassForCellReuse(typeof(TeamCellView), TeamCellView.Key);

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no teams."));

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(CreateSource)
                .BindTo(TableView, x => x.Source);
        }

        private ReactiveTableViewSource<TeamItemViewModel> CreateSource(IReadOnlyReactiveList<TeamItemViewModel> items)
        {
            var source = new ReactiveTableViewSource<TeamItemViewModel>(TableView, items, TeamCellView.Key, (float)UITableView.AutomaticDimension);
            source.ElementSelected.OfType<TeamItemViewModel>().Subscribe(x => x.GoToCommand.ExecuteIfCan());
            return source;
        }
    }
}