using System;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Delegates;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ReactiveTableViewController<RepositoriesExploreViewModel>
    {
        public RepositoriesExploreView()
        {
            Title = "Explore";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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

