using System;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Organizations;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        protected readonly ReactiveList<BasicUserModel> UsersCollection = new ReactiveList<BasicUserModel>();

        public IReadOnlyReactiveList<UserItemViewModel> Users { get; private set; }

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
            Users = UsersCollection.CreateDerivedCollection(
                x => 
                {
                    var isOrg = string.Equals(x.Type, "organization", StringComparison.OrdinalIgnoreCase);
                    return new UserItemViewModel(x.Login, x.AvatarUrl, isOrg, () =>
                    {
                        if (isOrg)
                        {
                            var vm = CreateViewModel<OrganizationViewModel>();
                            vm.Username = x.Login;
                            ShowViewModel(vm);
                        }
                        else
                        {
                            var vm = CreateViewModel<UserViewModel>();
                            vm.Username = x.Login;
                            ShowViewModel(vm);
                        }
                    });
                },
                x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));
        }
    }
}
