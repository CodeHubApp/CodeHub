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

        public string Username { get; private set; }

        public string AvatarUrl { get; private set; }

        public string Domain { get; private set; }

        public string Id { get; private set; }

        public IReactiveCommand<object> DeleteCommand { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

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

