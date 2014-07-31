using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using System;

namespace CodeHub.iOS.Views.Repositories
{
    public abstract class BaseRepositoriesView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : RepositoriesViewModel
    {
        protected BaseRepositoriesView()
            : base(unevenRows: true)
        {
            Title = "Repositories";

            //NoItemsText = "No Repositories"; 
//			NavigationItem.RightBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem(Theme.CurrentTheme.SortButton, MonoTouch.UIKit.UIBarButtonItemStyle.Plain, 
//				(s, e) => ShowFilterController(new RepositoriesFilterViewController(ViewModel.Repositories)));  
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.BindList(ViewModel.Repositories, CreateElement);
			TableView.SeparatorInset = new MonoTouch.UIKit.UIEdgeInsets(0, 56f, 0, 0);
        }

        protected Element CreateElement(RepositoryModel repo)
        {
            var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
            var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner.Login, new Uri(repo.Owner.AvatarUrl)) { ShowOwner = ViewModel.ShowRepositoryOwner };
            sse.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(repo);
            return sse;
        }
    }
}