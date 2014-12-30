using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using System.Reactive.Subjects;
using Xamarin.Utilities.ViewModels;
using CodeHub.Core.Data;
using System.Reactive;
using Xamarin.Utilities.Factories;
using System.Reactive.Linq;
using Xamarin.Utilities.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Subject<GistModel> _createdGistSubject = new Subject<GistModel>();
        private IDictionary<string, string> _files;

        public IObservable<GistModel> CreatedGist 
        { 
            get { return _createdGistSubject; } 
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        private bool _isPublic;
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

        public GitHubAccount CurrentAccount { get; private set; }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public IReactiveCommand<object> ModifyGistFileCommand { get; private set; }

        public IReactiveCommand<object> AddGistFileCommand { get; private set; }

        public GistCreateViewModel(IApplicationService applicationService, IAlertDialogFactory alertDialogFactory, 
            IStatusIndicatorService statusIndicatorService)
        {
            _applicationService = applicationService;
            CurrentAccount = applicationService.Account;

            Title = "Create Gist";
            Files = new Dictionary<string, string>();
            IsPublic = true;

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Files).Select(x => x.Count > 0),
                async t =>
            {
                var createGist = new GistCreateModel
                {
                    Description = Description,
                    Public = IsPublic,
                    Files = Files.ToDictionary(x => x.Key, x => new GistCreateModel.File { Content = x.Value })
                };

                var request = _applicationService.Client.AuthenticatedUser.Gists.CreateGist(createGist);
                using (statusIndicatorService.Activate("Creating Gist..."))
                    _createdGistSubject.OnNext((await _applicationService.Client.ExecuteAsync(request)).Data);
                Dismiss();
            });

            AddGistFileCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<ModifyGistViewModel>();
                vm.Title = "New File";
                vm.SaveCommand.Subscribe(__ =>
                {
                    if (Files.ContainsKey(vm.Filename))
                        alertDialogFactory.Alert("File already exists!", "Gist already contains a file with that name!");
                    else
                    {
                        Files.Add(vm.Filename, vm.Description);
                        Files = new Dictionary<string, string>(Files);
                        vm.Dismiss();
                    }
                });
                NavigateTo(vm);
            });

            ModifyGistFileCommand = ReactiveCommand.Create();

            // Analysis disable once ConvertClosureToMethodGroup
            // Don't remove the lambda below. It doesn't actually seem to work correctly
            ModifyGistFileCommand.OfType<string>().Where(x => Files.ContainsKey(x)).Subscribe(x =>
            {
                var vm = this.CreateViewModel<ModifyGistViewModel>();
                vm.Title = vm.Filename = x;
                vm.Description = Files[x];
                vm.SaveCommand.Subscribe(__ =>
                {
                    Files.Remove(x);
                    Files[vm.Filename] = vm.Description;
                    Files = new Dictionary<string, string>(Files);
                    vm.Dismiss();
                });
                NavigateTo(vm);
            });
        }
    }
}

