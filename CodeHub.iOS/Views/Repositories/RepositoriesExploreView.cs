using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.iOS.Utilities;
using CodeHub.Core.Utilities;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ViewModelCollectionDrivenDialogViewController
    {
        public RepositoriesExploreView()
        {
            Title = "Explore";

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories."));
        }

        protected override IUISearchBarDelegate CreateSearchDelegate()
        {
            return null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var vm = (RepositoriesExploreViewModel)ViewModel;
            var search = (UISearchBar)TableView.TableHeaderView;

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 64f;

            var weakVm = new WeakReference<RepositoriesExploreViewModel>(vm);
            BindCollection(vm.Repositories, repo =>
            {
                var description = vm.ShowRepositoryDescription ? repo.Description : string.Empty;
                var avatar = new GitHubAvatar(repo.Owner?.AvatarUrl);
                var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.ForksCount, description, repo.Owner.Login, avatar) { ShowOwner = true };
                sse.Tapped += MakeCallback(weakVm, repo);
                return sse;
            });

            OnActivation(d =>
            {
                d(search.GetChangedObservable().Subscribe(x => vm.SearchText = x));
                d(vm.Bind(x => x.IsSearching).SubscribeStatus("Searching..."));
                d(search.GetSearchObservable().Subscribe(_ => {
                    search.ResignFirstResponder();
                    vm.SearchCommand.Execute(null);
                }));
            });
        }

        private static Action MakeCallback(WeakReference<RepositoriesExploreViewModel> weakVm, RepositorySearchModel.RepositoryModel model)
        {
            return new Action(() => weakVm.Get()?.GoToRepositoryCommand.Execute(model));
        }
    }
}

