using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.Data;
using System.Reactive.Linq;
using CodeHub.Core.Factories;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCreateViewModel : BaseViewModel
    {
        private readonly ISessionService _applicationService;

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

        public IReadOnlyReactiveList<GistFileItemViewModel> Files { get; private set; }

        public GitHubAccount CurrentAccount { get; private set; }

        public IReactiveCommand<GistModel> SaveCommand { get; private set; }

        public IReactiveCommand<object> AddGistFileCommand { get; private set; }

        public GistCreateViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory)
        {
            _applicationService = applicationService;
            CurrentAccount = applicationService.Account;

            Title = "Create Gist";
            IsPublic = true;

            var files = new ReactiveList<Tuple<string, string>>();
            Files = files.CreateDerivedCollection(x => 
            {
                var item = new GistFileItemViewModel(x.Item1, x.Item2);
                item.EditCommand.Subscribe(_ => NavigateTo(new GistFileEditViewModel(y =>
                {
                    var i = files.IndexOf(x);
                    files.Remove(x);
                    files.Insert(i, y);
                    return Task.FromResult(0);
                })
                {
                    Filename = x.Item1,
                    Description = x.Item2
                }));
                item.DeleteCommand.Subscribe(_ => files.Remove(x));
                return item;
            });

            SaveCommand = ReactiveCommand.CreateAsyncTask(Files.CountChanged.Select(x => x > 0),
                async t =>
                {
                    var createGist = new GistCreateModel
                    {
                        Description = Description,
                        Public = IsPublic,
                        Files = Files.ToDictionary(x => x.Name, x => new GistCreateModel.File { Content = x.Content })
                    };

                    var request = _applicationService.Client.AuthenticatedUser.Gists.CreateGist(createGist);
                    using (alertDialogFactory.Activate("Creating Gist..."))
                        return (await _applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(_ => Dismiss());

            AddGistFileCommand = ReactiveCommand.Create()
                .WithSubscription(_ => NavigateTo(new GistFileAddViewModel(x =>
                {
                    if (Files.Any(y => y.Name == x.Item1))
                        throw new Exception("Gist already contains a file with that name!");
                    files.Add(x);
                    return Task.FromResult(0);
                })));
        }
    }
}

