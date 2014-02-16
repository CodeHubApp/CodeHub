using System;
using CodeFramework.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Linq;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel 
    {
        private string _description;
        private bool _public;
        private IDictionary<string, string> _files;
        private bool _saving;

        public bool IsSaving
        {
            get { return _saving; }
            private set
            {
                _saving = value;
                RaisePropertyChanged(() => IsSaving);
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged(() => Description);
            }
        }

        public bool Public
        {
            get { return _public; }
            set
            {
                _public = value;
                RaisePropertyChanged(() => Public);
            }
        }

        public IDictionary<string, string> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                RaisePropertyChanged(() => Files);
            }
        }

        public ICommand SaveCommand
        {
            get { return new MvxCommand(() => Save()); }
        }
            
        public void Init()
        {
            var createGistModel = GetService<CodeFramework.Core.Services.IViewModelTxService>().Get() as GistCreateModel;
            if (createGistModel != null)
            {
                Description = createGistModel.Description;
                Public = createGistModel.Public ?? false;
                Files = createGistModel.Files != null ? 
                    createGistModel.Files.ToDictionary(x => x.Key, x => x.Value.Content) :
                    new Dictionary<string, string>();
            }
            else
            {
                Files = new Dictionary<string, string>();
            }
        }

        private async Task Save()
        {
            if (_files.Count == 0)
            {
                DisplayException(new Exception("You cannot create a Gist without atleast one file"));
                return;
            }

            try
            {
                var createGist = new GistCreateModel()
                {
                    Description = Description,
                    Public = Public,
                    Files = Files.ToDictionary(x => x.Key, x => new GistCreateModel.File { Content = x.Value })
                };

                IsSaving = true;
                var newGist = (await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Gists.CreateGist(createGist))).Data;
                Messenger.Publish(new GistAddMessage(this, newGist));
                ChangePresentation(new MvxClosePresentationHint(this));
            }
            catch (Exception e)
            {
                ReportError(e);
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

