using CodeFramework.iOS.Elements;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using ReactiveUI;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : ViewModelCollectionView<TViewModel> where TViewModel : RepositoriesViewModel
    {
        protected BaseRepositoriesView()
        {
            Title = "Repositories";
            NoItemsText = "No Repositories"; 
//			NavigationItem.RightBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem(Theme.CurrentTheme.SortButton, MonoTouch.UIKit.UIBarButtonItemStyle.Plain, 
//				(s, e) => ShowFilterController(new RepositoriesFilterViewController(ViewModel.Repositories)));  
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Bind(ViewModel.WhenAnyValue(x => x.Repositories), CreateElement);
			TableView.SeparatorInset = new MonoTouch.UIKit.UIEdgeInsets(0, 56f, 0, 0);
        }

        protected Element CreateElement(RepositoryModel repo)
        {
            var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
            var imageUrl = repo.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl;
            var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner.Login, imageUrl) { ShowOwner = ViewModel.ShowRepositoryOwner };
            sse.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(repo);
            return sse;
        }
    }
}