using System;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly ReactiveList<BasicUserModel> UsersCollection = new ReactiveList<BasicUserModel>();

        public IReadOnlyReactiveList<UserViewModel> Users { get; private set; }

        private IReactiveCommand _loadMore;
        public IReactiveCommand LoadMore
        {
            get { return _loadMore; }
            protected set { this.RaiseAndSetIfChanged(ref _loadMore, value); }
        }

        public IReactiveCommand LoadCommand { get; protected set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected Task Load(GitHubRequest<List<BasicUserModel>> request, bool? forceCacheInvalidation = false)
        {
            return UsersCollection.SimpleCollectionLoad(request, forceCacheInvalidation, 
                x => LoadMore = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
        }

        protected BaseUserCollectionViewModel()
        {
            var gotoUser = new Action<UserViewModel>(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = x.Name;
                ShowViewModel(vm);
            });

            Users = UsersCollection.CreateDerivedCollection(
                x => new UserViewModel(x.Login, x.AvatarUrl, gotoUser), 
                x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));
        }
    }
}
