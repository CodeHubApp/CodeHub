using System;
using CodeFramework.iOS.Elements;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ViewModelCollectionView<RepositoriesExploreViewModel>
    {
        private readonly IStatusIndicatorService _statusIndicatorService;

        public RepositoriesExploreView(IStatusIndicatorService statusIndicatorService)
		{
		    _statusIndicatorService = statusIndicatorService;
		    AutoHideSearch = false;
		}

        public override void ViewDidLoad()
        {
            NoItemsText = "No Repositories";
            Title = "Explore";

            base.ViewDidLoad();

            var search = (UISearchBar)TableView.TableHeaderView;
            search.TextChanged += (sender, args) => ViewModel.SearchText = search.Text;
			search.SearchButtonClicked += (sender, e) =>
			{
				search.ResignFirstResponder();
				ViewModel.SearchCommand.ExecuteIfCan();
			};

			ViewModel.SearchCommand.IsExecuting.Subscribe(x =>
			{
				if (x)
                    _statusIndicatorService.Show("Searching...");
				else
                    _statusIndicatorService.Hide();
			});

			Bind(ViewModel.WhenAnyValue(x => x.Repositories), repo =>
            {
				var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
                var imageUrl = repo.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl;
				var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.ForksCount, description, repo.Owner.Login, imageUrl) { ShowOwner = true };
				sse.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(repo);
                return sse;
            });
        }
    }
}

