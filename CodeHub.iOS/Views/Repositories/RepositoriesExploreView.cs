using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels;
using UIKit;
using CodeHub.iOS.Utilities;
using MvvmCross.Binding.BindingContext;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : ViewModelCollectionDrivenDialogViewController
    {
		public RepositoriesExploreView()
        {
            AutoHideSearch = false;
            NoItemsText = "No Repositories";
            Title = "Explore";
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
            TableView.SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);

			var set = this.CreateBindingSet<RepositoriesExploreView, RepositoriesExploreViewModel>();
			set.Bind(search).For(x => x.Text).To(x => x.SearchText);
			set.Apply();

			search.SearchButtonClicked += (sender, e) =>
			{
				search.ResignFirstResponder();
				vm.SearchCommand.Execute(null);
			};

            vm.Bind(x => x.IsSearching).SubscribeStatus("Searching...");

            var weakVm = new WeakReference<RepositoriesExploreViewModel>(vm);
			BindCollection(vm.Repositories, repo =>
            {
				var description = vm.ShowRepositoryDescription ? repo.Description : string.Empty;
                var avatar = new GitHubAvatar(repo.Owner?.AvatarUrl);
                var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.ForksCount, description, repo.Owner.Login, avatar) { ShowOwner = true };
                sse.Tapped += () => weakVm.Get()?.GoToRepositoryCommand.Execute(repo);
                return sse;
            });
        }
    }
}

