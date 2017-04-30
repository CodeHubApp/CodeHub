using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel 
    {
        private readonly IMessageService _messageService;
        private string _description;
        private bool _public;
        private IDictionary<string, string> _files = new Dictionary<string, string>();
        private bool _saving;

        public bool IsSaving
        {
            get { return _saving; }
            private set { this.RaiseAndSetIfChanged(ref _saving, value); }
        }

        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        public bool Public
        {
            get { return _public; }
            set { this.RaiseAndSetIfChanged(ref _public, value); }
        }

        public IDictionary<string, string> Files
        {
            get { return _files; }
            set { this.RaiseAndSetIfChanged(ref _files, value); }
        }

        public ReactiveCommand<Unit, Gist> SaveCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }


        public GistCreateViewModel(IMessageService messageService = null)
        {
            _messageService = messageService ?? GetService<IMessageService>();

            CancelCommand = ReactiveCommand.Create(() => { });
            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            SaveCommand.ThrownExceptions.Subscribe(x => DisplayAlert(x.Message));
        }

        private async Task<Gist> Save()
        {
            if (_files.Count == 0)
                throw new Exception("You cannot create a Gist without atleast one file! Please correct and try again.");

            try
            {
                var newGist = new NewGist()
                {
                    Description = Description ?? string.Empty,
                    Public = Public
                };

                foreach (var kv in Files)
                    newGist.Files.Add(kv.Key, kv.Value);

                IsSaving = true;
                var gist = await this.GetApplication().GitHubClient.Gist.Create(newGist);
                _messageService.Send(new GistAddMessage(gist));
                return gist;
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

