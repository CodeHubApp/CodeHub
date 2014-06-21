using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private string _description;
        private bool _public;
        private IDictionary<string, string> _files;

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

        private GistModel _createdGist;
        public GistModel CreatedGist
        {
            get { return _createdGist; }
            private set { this.RaiseAndSetIfChanged(ref _createdGist, value); }
        }

        public IReactiveCommand SaveCommand { get; private set; }

        public GistCreateViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            SaveCommand = new ReactiveCommand();
            SaveCommand.RegisterAsyncTask(async t =>
            {
                if (_files == null || _files.Count == 0)
                    throw new Exception("You cannot create a Gist without atleast one file! Please correct and try again.");

                var createGist = new GistCreateModel
                {
                    Description = Description,
                    Public = Public,
                    Files = Files.ToDictionary(x => x.Key, x => new GistCreateModel.File { Content = x.Value })
                };

                var newGist = (await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Gists.CreateGist(createGist))).Data;
                CreatedGist = newGist;
                DismissCommand.ExecuteIfCan();
            });
        }
    }
}

