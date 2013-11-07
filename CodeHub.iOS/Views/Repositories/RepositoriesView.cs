using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class RepositoriesView : ViewModelCollectionDrivenViewController
    {
        public bool ShowOwner { get; set; }

        public new RepositoriesViewModel ViewModel
        {  
            get { return (RepositoriesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        protected RepositoriesView()
        {
            ShowOwner = false;
            EnableFilter = true;
            Title = "Repositories".t();
            SearchPlaceholder = "Search Repositories".t();
            NoItemsText = "No Repositories".t();   
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            BindCollection(ViewModel.Repositories, CreateElement);
        }

        protected Element CreateElement(RepositoryModel repo)
        {
            var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
            var imageUrl = repo.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl;
            var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner.Login, imageUrl) { ShowOwner = ShowOwner };
            //sse.Tapped += () => NavigationController.PushViewController(new RepositoryViewController(repo.Owner.Login, repo.Name), true);
            return sse;
        }

//        protected override FilterViewController CreateFilterController()
//        {
//            return new RepositoriesFilterViewController(ViewModel.Repositories);
//        }
    }
}