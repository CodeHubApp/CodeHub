using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using System.Reactive.Subjects;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<GistModel> _createdGistSubject = new Subject<GistModel>();
        private string _description;
        private bool _isPublic;
        private IDictionary<string, string> _files;

        public IObservable<GistModel> CreatedGist 
        { 
            get { return _createdGistSubject; } 
        }

        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        public bool IsPublic
        {
            get { return _isPublic; }
            set { this.RaiseAndSetIfChanged(ref _isPublic, value); }
        }

        public IDictionary<string, string> Files
        {
            get { return _files; }
            set { this.RaiseAndSetIfChanged(ref _files, value); }
        }

        public IReactiveCommand SaveCommand { get; private set; }

        public GistCreateViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            Title = "Create Gist";
            Files = new Dictionary<string, string>();

            SaveCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (_files == null || _files.Count == 0)
                    throw new Exception("You cannot create a Gist without atleast one file! Please correct and try again.");

                var createGist = new GistCreateModel
                {
                    Description = Description,
                    Public = IsPublic,
                    Files = Files.ToDictionary(x => x.Key, x => new GistCreateModel.File { Content = x.Value })
                };

                var request = _applicationService.Client.AuthenticatedUser.Gists.CreateGist(createGist);
                _createdGistSubject.OnNext((await _applicationService.Client.ExecuteAsync(request)).Data);
                Dismiss();
            });
        }
    }
}

