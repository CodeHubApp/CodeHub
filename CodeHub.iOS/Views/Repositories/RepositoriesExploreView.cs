using System;
using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ViewModelCollectionViewController<RepositoriesExploreViewModel>
    {
        public RepositoriesExploreView()
            : base(unevenRows: true)
		{
            Title = "Explore";
		    AutoHideSearch = false;
            //NoItemsText = "No Repositories";

            this.WhenActivated(d =>
            {
                d(SearchTextChanged.Subscribe(x => 
                {
                    ViewModel.SearchText = x;
                    ViewModel.SearchCommand.ExecuteIfCan();
                }));
            });
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.BindList(ViewModel.Repositories, repo =>
            {
				var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
                var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.ForksCount, description, repo.Owner.Login, new Uri(repo.Owner.AvatarUrl)) { ShowOwner = true };
				sse.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(repo);
                return sse;
            });
        }
    }
}

