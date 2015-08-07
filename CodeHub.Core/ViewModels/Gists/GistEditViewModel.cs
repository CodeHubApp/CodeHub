using System;
using CodeHub.Core.Services;
using System.Linq;
using CodeHub.Core.Factories;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistEditViewModel : GistModifyViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly IAlertDialogFactory _alertDialogFactory;
        private DateTime _lastChanged;
        private DateTime _lastLoaded;

        private Gist  _gist;
        public Gist Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        public GistEditViewModel(ISessionService sessionService, IAlertDialogFactory alertDialogFactory)
        {
            _sessionService = sessionService;
            _alertDialogFactory = alertDialogFactory;

            Title = "Edit Gist";

            this.WhenAnyValue(x => x.Gist)
                .IsNotNull()
                .Subscribe(x =>
                {
                    Description = x.Description;
                    foreach (var file in x.Files)
                        InternalFiles.Add(Tuple.Create(file.Key, file.Value.Content));
                    _lastLoaded = DateTime.Now;
                });

            this.WhenAnyValue(x => x.Description).Select(x => Unit.Default)
                .Merge(InternalFiles.Changed.Select(x => Unit.Default))
                .Subscribe(_ => _lastChanged = DateTime.Now);
        }

        protected override async System.Threading.Tasks.Task<Gist> SaveGist()
        {
            if (Gist == null)
                throw new InvalidOperationException("Missing Gist context to update!");

            var gistUpdate = new GistUpdate { Description = Description ?? string.Empty };

            foreach (var file in Gist.Files)
                gistUpdate.Files[file.Key] = null;

            foreach (var file in Files)
                gistUpdate.Files[file.Name] = new GistFileUpdate { Content = file.Content };

            using (_alertDialogFactory.Activate("Updating Gist..."))
                return await _sessionService.GitHubClient.Gist.Edit(Gist.Id, gistUpdate);
        }

        protected override async System.Threading.Tasks.Task<bool> Discard()
        {
            if (_lastChanged <= _lastLoaded) return true;
            return await _alertDialogFactory.PromptYesNo("Discard Gist?", "Are you sure you want to discard this gist?");
        }
    }
}