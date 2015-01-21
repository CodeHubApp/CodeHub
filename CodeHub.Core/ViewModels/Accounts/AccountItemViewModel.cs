using ReactiveUI;
using CodeHub.Core.Data;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        private GitHubAccount _account;
        public GitHubAccount Account
        {
            get { return _account; }
            internal set { this.RaiseAndSetIfChanged(ref _account, value); }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            internal set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _username;
        public string Username
        {
            get { return _username.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _avatarUrl;
        public string AvatarUrl
        {
            get { return _avatarUrl.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _domain;
        public string Domain
        {
            get { return _domain.Value; }
        }

        public IReactiveCommand<object> DeleteCommand { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal AccountItemViewModel()
        {
            DeleteCommand = ReactiveCommand.Create();
            GoToCommand = ReactiveCommand.Create();

            var accountObservable = this.WhenAnyValue(x => x.Account).IsNotNull();

            _username = accountObservable.Select(x => x.Username).ToProperty(this, x => x.Username);
            _avatarUrl = accountObservable.Select(x => x.AvatarUrl).ToProperty(this, x => x.AvatarUrl);
            _domain = accountObservable.Select(x => x.WebDomain ?? "https://api.github.com").ToProperty(this, x => x.Domain);
        }
    }
}

