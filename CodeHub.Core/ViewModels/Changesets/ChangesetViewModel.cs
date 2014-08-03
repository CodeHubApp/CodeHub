using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetViewModel : BaseViewModel, ILoadableViewModel
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

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFileCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public ReactiveList<CommentModel> Comments { get; private set; }
        
        public ChangesetViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            Comments = new ReactiveList<CommentModel>();

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Commit).Select(x => x != null));
            GoToHtmlUrlCommand.Select(x => Commit).Subscribe(GoToUrlCommand.ExecuteIfCan);

            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                ShowViewModel(vm);
            });

            GoToFileCommand = ReactiveCommand.Create();
            GoToFileCommand.OfType<CommitModel.CommitFileModel>().Subscribe(x =>
            {
                if (x.Patch == null)
                {
                    var vm = CreateViewModel<SourceViewModel>();
                    vm.Branch = Commit.Sha;
                    vm.Username = RepositoryOwner;
                    vm.Repository = RepositoryName;
                    vm.Items = new [] 
                    { 
                        new SourceViewModel.SourceItemModel 
                        {
                            ForceBinary = true,
                            GitUrl = x.BlobUrl,
                            Name = x.Filename,
                            Path = x.Filename,
                            HtmlUrl = x.BlobUrl
                        }
                    };
                    vm.CurrentItemIndex = 0;
                    ShowViewModel(vm);
                }
                else
                {
                    var vm = CreateViewModel<ChangesetDiffViewModel>();
                    vm.Username = RepositoryOwner;
                    vm.Repository = RepositoryName;
                    vm.Branch = Commit.Sha;
                    vm.Filename = x.Filename;
                    ShowViewModel(vm);
                }
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
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

