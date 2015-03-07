using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : GistModifyViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly IAlertDialogFactory _alertDialogFactory;

        private bool _isPublic;
        public bool IsPublic
        {
            get { return _isPublic; }
            set { this.RaiseAndSetIfChanged(ref _isPublic, value); }
        }

        public GistCreateViewModel(ISessionService sessionService, IAlertDialogFactory alertDialogFactory)
        {
            _sessionService = sessionService;
            _alertDialogFactory = alertDialogFactory;

            Title = "Create Gist";
            IsPublic = true;
        }

        protected override async System.Threading.Tasks.Task<GistModel> SaveGist()
        {
            var createGist = new GistCreateModel
            {
                Description = Description ?? string.Empty,
                Public = IsPublic,
                Files = Files.ToDictionary(x => x.Name.Trim(), x => new GistCreateModel.File { Content = x.Content })
            };

            var request = _sessionService.Client.AuthenticatedUser.Gists.CreateGist(createGist);
            using (_alertDialogFactory.Activate("Creating Gist..."))
                return (await _sessionService.Client.ExecuteAsync(request)).Data;
        }
    }
}

