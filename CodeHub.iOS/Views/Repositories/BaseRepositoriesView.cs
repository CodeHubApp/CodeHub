using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeHub.iOS.Views.Filters;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView : ViewModelCollectionDrivenDialogViewController
    {
        public new RepositoriesViewModel ViewModel
        {  
            get { return (RepositoriesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        protected BaseRepositoriesView()
        {
            Title = "Repositories".t();
            NoItemsText = "No Repositories".t(); 
			NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem(Theme.CurrentTheme.SortButton, UIKit.UIBarButtonItemStyle.Plain, 
				(s, e) => ShowFilterController(new RepositoriesFilterViewController(ViewModel.Repositories)));  
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            BindCollection(ViewModel.Repositories, CreateElement);
			TableView.SeparatorInset = new UIKit.UIEdgeInsets(0, 56f, 0, 0);
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