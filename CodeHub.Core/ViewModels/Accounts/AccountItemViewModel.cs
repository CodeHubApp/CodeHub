using ReactiveUI;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            internal set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public string Username { get; }

        public string AvatarUrl { get; }

        public string Domain { get; }

        public string Id { get; }

        public IReactiveCommand<object> DeleteCommand { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        internal AccountItemViewModel(GitHubAccount account)
        {
            DeleteCommand = ReactiveCommand.Create();
            GoToCommand = ReactiveCommand.Create();

            Id = account.Key;
            Username = account.Username;
            AvatarUrl = account.AvatarUrl;
            Domain = account.WebDomain ?? "https://api.github.com";
        }
    }
}

