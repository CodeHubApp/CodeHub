using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : BaseGistsViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        private string _username;
        public string Username
        {
            get { return _username; }
            set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        public bool IsMine
        {
			get { return _applicationService.Account.Username.Equals(Username); }
        }

        public IReactiveCommand<object> GoToCreateGistCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public UserGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Username = _applicationService.Account.Username;

            GoToCreateGistCommand = ReactiveCommand.Create();
            GoToCreateGistCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<GistCreateViewModel>();
                ShowViewModel(vm);
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

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                GistsCollection.SimpleCollectionLoad(applicationService.Client.Users[Username].Gists.GetGists(), t as bool?));
        }
    }
}
