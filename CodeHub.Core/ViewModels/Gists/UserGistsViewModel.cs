using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : GistsViewModel
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

        public IReactiveCommand GoToCreateGistCommand { get; private set; }

        public UserGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Username = _applicationService.Account.Username;

            GoToCreateGistCommand = new ReactiveCommand();
            GoToCreateGistCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<GistCreateViewModel>();
                ShowViewModel(vm);
            });
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
			return _applicationService.Client.Users[Username].Gists.GetGists();
        }
    }

}
