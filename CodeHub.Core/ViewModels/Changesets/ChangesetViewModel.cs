using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;

		public string Node { get; set; }

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public bool ShowRepository { get; set; }

        private CommitModel _commitModel;
        public CommitModel Commit
        {
            get { return _commitModel; }
            private set { this.RaiseAndSetIfChanged(ref _commitModel, value); }
        }

        public IReactiveCommand GoToFileCommand { get; private set; }

        public IReactiveCommand GoToRepositoryCommand { get; private set; }

		public IReactiveCommand GoToHtmlUrlCommand { get; private set; }

        public ReactiveCollection<CommentModel> Comments { get; private set; }
        
        public ChangesetViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            Comments = new ReactiveCollection<CommentModel>();

            GoToHtmlUrlCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Commit, x => x != null));
            GoToHtmlUrlCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<WebBrowserViewModel>();
                vm.Url = Commit.Url;
                ShowViewModel(vm);
            });

            GoToRepositoryCommand = new ReactiveCommand();
            GoToRepositoryCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToFileCommand = new ReactiveCommand();
            GoToFileCommand.OfType<CommitModel.CommitFileModel>().Subscribe(x =>
            {
                if (x.Patch == null)
                {
                    var vm = CreateViewModel<SourceViewModel>();
                    vm.GitUrl = x.ContentsUrl;
                    vm.HtmlUrl = x.BlobUrl;
                    vm.Name = x.Filename;
                    vm.Path = x.Filename;
                    vm.ForceBinary = true;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<ChangesetDiffViewModel>();
                    vm.Username = RepositoryOwner;
                    vm.Repository = RepositoryName;
                    vm.Branch = Commit.Sha;
                    vm.HtmlUrl = x.BlobUrl;
                    ShowViewModel(vm);
                }
            });

            LoadCommand.RegisterAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var t1 = this.RequestModel(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Get(), forceCacheInvalidation, response => Commit = response.Data);
                Comments.SimpleCollectionLoad(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.GetAll(), forceCacheInvalidation).FireAndForget();
                return t1;
            });
        }

        public async Task AddComment(string text)
        {
            var c = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.Create(text));
            Comments.Add(c.Data);
        }
    }
}

