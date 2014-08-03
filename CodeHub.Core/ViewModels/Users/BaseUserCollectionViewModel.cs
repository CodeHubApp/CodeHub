using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class BaseUserCollectionViewModel : BaseViewModel
    {
        protected readonly ReactiveList<BasicUserModel> UsersCollection = new ReactiveList<BasicUserModel>();

        public IReadOnlyReactiveList<BasicUserModel> Users { get; private set; }

        public IReactiveCommand<object> GoToUserCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseUserCollectionViewModel()
        {
            Users = UsersCollection.CreateDerivedCollection(
                x => x, x => x.Login.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToUserCommand = ReactiveCommand.Create();
            GoToUserCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = x.Login;
                ShowViewModel(vm);
            });
        }
    }
}
