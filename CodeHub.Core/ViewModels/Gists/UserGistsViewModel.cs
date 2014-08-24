using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : BaseGistsViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; set; }

        public string Title
        {
            get
            {
                if (Username == null) 
                    return "Gists";
                if (IsMine)
                    return "My Gists";
                if (Username.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    return Username + "' Gists";
                return Username + "'s Gists";
            }
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

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                GistsCollection.SimpleCollectionLoad(applicationService.Client.Users[Username].Gists.GetGists(), t as bool?));
            LoadCommand.ExecuteIfCan();
        }
    }
}
