using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Reactive;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Factories;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitViewModel : BaseViewModel, ILoadableViewModel, ICanGoToUrl
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

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFileCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand GoToCommentCommand { get; private set; }

        public ReactiveList<CommentModel> Comments { get; private set; }

        public IReactiveCommand GoToUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }
        
        public CommitViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuService)
        {
            _applicationService = applicationService;

            Title = "Commit";

            Comments = new ReactiveList<CommentModel>();

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Commit).Select(x => x != null));
            GoToHtmlUrlCommand.Select(x => Commit.HtmlUrl).Subscribe(x => 
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            GoToCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<CommitCommentViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Node = Node;
                vm.CommentAdded.Subscribe(Comments.Add);
                NavigateTo(vm);
            });

            GoToFileCommand = ReactiveCommand.Create();
            GoToFileCommand.OfType<CommitModel.CommitFileModel>().Subscribe(x =>
            {
                if (x.Patch == null)
                {
                    var vm = this.CreateViewModel<SourceViewModel>();
                    vm.Branch = Commit.Sha;
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
//                    vm.Items = new [] 
//                    { 
//                        new SourceViewModel.SourceItemModel 
//                        {
//                            ForceBinary = true,
//                            GitUrl = x.BlobUrl,
//                            Name = x.Filename,
//                            Path = x.Filename,
//                            HtmlUrl = x.BlobUrl
//                        }
//                    };
//                    vm.CurrentItemIndex = 0;
                    NavigateTo(vm);
                }
                else
                {
                    var vm = this.CreateViewModel<ChangesetDiffViewModel>();
                    vm.Username = RepositoryOwner;
                    vm.Repository = RepositoryName;
                    vm.Branch = Commit.Sha;
                    vm.Filename = x.Filename;
                    NavigateTo(vm);
                }
            });

            var copyShaCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Commit).Select(x => x != null))
                .WithSubscription(x => actionMenuService.SendToPasteBoard(this.Commit.Sha));

            var shareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Commit).Select(x => x != null))
                .WithSubscription(x => actionMenuService.ShareUrl(this.Commit.HtmlUrl));

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                var menu = actionMenuService.Create(Title);
                menu.AddButton("Add Comment", GoToCommentCommand);
                menu.AddButton("Copy SHA", copyShaCommand);
                menu.AddButton("Share", shareCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var t1 = this.RequestModel(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Get(), forceCacheInvalidation, response => Commit = response.Data);
                Comments.SimpleCollectionLoad(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.GetAll(), forceCacheInvalidation);
                return t1;
            });
        }
    }
}

