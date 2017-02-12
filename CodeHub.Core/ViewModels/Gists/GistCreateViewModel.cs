using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Linq;
using CodeHub.Core.Messages;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;

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

        public ReactiveCommand<Unit, GistModel> SaveCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }


        public GistCreateViewModel(IMessageService messageService = null)
        {
            _messageService = messageService ?? GetService<IMessageService>();

            CancelCommand = ReactiveCommand.Create(() => { });
            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            SaveCommand.ThrownExceptions.Subscribe(x => DisplayAlert(x.Message));
        }

        private async Task<GistModel> Save()
        {
            if (_files.Count == 0)
                throw new Exception("You cannot create a Gist without atleast one file! Please correct and try again.");

            try
            {
                var createGist = new GistCreateModel
                {
                    Description = Description ?? string.Empty,
                    Public = Public,
                    Files = Files.ToDictionary(x => x.Key, x => new GistCreateModel.File { Content = x.Value })
                };

                IsSaving = true;
                var newGist = (await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Gists.CreateGist(createGist))).Data;
                _messageService.Send(new GistAddMessage(newGist));
                return newGist;
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

