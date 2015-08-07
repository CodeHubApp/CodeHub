using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : BaseGistsViewModel
    {
        private readonly ISessionService _sessionService;

        private string _username;
        public string Username
        {
            get { return _username; }
            private set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        public bool IsMine
        {
			get { return _sessionService.Account.Username.Equals(Username); }
        }

        public IReactiveCommand<object> GoToCreateGistCommand { get; private set; }

        public UserGistsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            _sessionService = sessionService;
            Username = _sessionService.Account.Username;

            GoToCreateGistCommand = ReactiveCommand.Create();
            GoToCreateGistCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<GistCreateViewModel>();
                vm.SaveCommand
                    .Delay(TimeSpan.FromMilliseconds(200))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => InternalGists.Insert(0, x));
                NavigateTo(vm);
            });

            this.WhenAnyValue(x => x.Username).Subscribe(x =>
            {
                if (IsMine)
                    Title = "My Gists";
                else if (x == null) 
                    Title = "Gists";
                else if (x.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    Title = x + "' Gists";
                else
                    Title = x + "'s Gists";
            });
        }

        protected override Uri RequestUri
        {
            get { return Octokit.ApiUrls.UsersGists(Username); }
        }

        public UserGistsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}
