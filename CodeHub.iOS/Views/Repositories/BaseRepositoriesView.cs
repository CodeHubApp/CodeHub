using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using UIKit;
using System;
using CodeHub.Core.Utilities;
using CodeHub.iOS.Utilities;

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
            Title = "Repositories";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            BindCollection(ViewModel.Repositories, CreateElement);

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 64f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        protected Element CreateElement(RepositoryModel repo)
        {
            var description = ViewModel.ShowRepositoryDescription ? Emojis.FindAndReplace(repo.Description) : string.Empty;
            var avatar = new GitHubAvatar(repo.Owner?.AvatarUrl);
            var vm = new WeakReference<RepositoriesViewModel>(ViewModel);
            var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner.Login, avatar) { ShowOwner = ViewModel.ShowRepositoryOwner };
            sse.Tapped += () => vm.Get()?.GoToRepositoryCommand.Execute(repo);
            return sse;
        }
    }
}