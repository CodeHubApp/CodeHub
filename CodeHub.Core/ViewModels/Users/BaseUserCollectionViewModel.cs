using System;
using GitHubSharp.Models;
using ReactiveUI;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Organizations;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
    {
        public IReadOnlyReactiveList<UserItemViewModel> Users { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; protected set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseUserCollectionViewModel()
        {
            var users = new ReactiveList<BasicUserModel>();;
            Users = users.CreateDerivedCollection(
                x => 
                {
                    var isOrg = string.Equals(x.Type, "organization", StringComparison.OrdinalIgnoreCase);
                    return new UserItemViewModel(x.Login, x.AvatarUrl, isOrg, () =>
                    {
                        if (isOrg)
                        {
                            
                            var vm = this.CreateViewModel<OrganizationViewModel>();
                            vm.Username = x.Login;
                            NavigateTo(vm);
                        }
                        else
                        {
                            var vm = this.CreateViewModel<UserViewModel>();
                            vm.Username = x.Login;
                            NavigateTo(vm);
                        }
                    });
                },
                x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                users.SimpleCollectionLoad(CreateRequest(), t as bool?, 
                x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x())));
        }

        protected abstract GitHubRequest<List<BasicUserModel>> CreateRequest();
    }
}
