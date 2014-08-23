using System;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Delegates;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ReactiveTableViewController<RepositoriesExploreViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Explore";

            base.ViewDidLoad();

            TableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);

            var searchDelegate = this.AddSearchBar();

            this.WhenActivated(d =>
            {
                d(searchDelegate.SearchTextChanged.Subscribe(x => 
                {
                    ViewModel.SearchText = x;
                    ViewModel.SearchCommand.ExecuteIfCan();
                }));
            });
        }
    }
}

