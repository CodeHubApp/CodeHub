using CodeHub.Core.Services;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.Factories;
using Octokit;

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

        protected override async System.Threading.Tasks.Task<bool> Discard()
        {
            if (string.IsNullOrEmpty(Description) && Files.Count == 0)
                return true;
            return await _alertDialogFactory.PromptYesNo("Discard Gist?", "Are you sure you want to discard this gist?");
        }

        protected override async System.Threading.Tasks.Task<Gist> SaveGist()
        {
            var newGist = new NewGist { Description = Description ?? string.Empty, Public = IsPublic };

            foreach (var file in Files)
                newGist.Files[file.Name.Trim()] = file.Content;

            using (_alertDialogFactory.Activate("Creating Gist..."))
                return await _sessionService.GitHubClient.Gist.Create(newGist);
        }
    }
}

