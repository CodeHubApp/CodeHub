using System;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel, ILoadableViewModel
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
            var gotoUser = new Action<UserItemViewModel>(x =>
            {
                if (x.IsOrganization)
                {
                    var vm = CreateViewModel<OrganizationViewModel>();
                    vm.Username = x.Name;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<UserViewModel>();
                    vm.Username = x.Name;
                    ShowViewModel(vm);
                }
            });

            Users = UsersCollection.CreateDerivedCollection(
                x => new UserItemViewModel(x.Login, x.AvatarUrl, string.Equals(x.Type, "organization", StringComparison.OrdinalIgnoreCase), gotoUser), 
                x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));
        }
    }
}
