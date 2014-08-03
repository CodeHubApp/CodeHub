using System;
using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ViewModelCollectionViewController<RepositoriesExploreViewModel>
    {
        public RepositoriesExploreView()
		{
            Title = "Explore";
		    AutoHideSearch = false;
            //NoItemsText = "No Repositories";
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var search = (UISearchBar)TableView.TableHeaderView;
            search.TextChanged += (sender, args) => ViewModel.SearchText = search.Text;
			search.SearchButtonClicked += (sender, e) =>
			{
				search.ResignFirstResponder();
				ViewModel.SearchCommand.ExecuteIfCan();
			};

            this.BindList(ViewModel.Repositories, repo =>
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

