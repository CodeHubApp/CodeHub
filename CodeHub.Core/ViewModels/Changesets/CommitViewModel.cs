using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Reactive;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Factories;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitViewModel : BaseViewModel, ILoadableViewModel, ICanGoToUrl
    {
		public string Node { get; set; }

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public bool ShowRepository { get; set; }

        private Octokit.GitHubCommit _commitModel;
        public Octokit.GitHubCommit Commit
        {
            get { return _commitModel; }
            private set { this.RaiseAndSetIfChanged(ref _commitModel, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFileCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand GoToCommentCommand { get; private set; }

        public IReadOnlyReactiveList<Octokit.CommitComment> Comments { get; private set; }

        public IReactiveCommand GoToUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }
        
        public CommitViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuService)
        {
            Title = "Commit";

            var comments = new ReactiveList<Octokit.CommitComment>();
            Comments = comments.CreateDerivedCollection(x => x);

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
                vm.SaveCommand.Subscribe(comments.Add);
                NavigateTo(vm);
            });

            GoToFileCommand = ReactiveCommand.Create();
            GoToFileCommand.OfType<Octokit.GitHubCommitFile>().Subscribe(x =>
            {
                if (x.Patch == null)
                {
                    var vm = this.CreateViewModel<SourceViewModel>();
                    vm.Branch = Commit.Sha;
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
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

            var validCommitObservable = this.WhenAnyValue(x => x.Commit).Select(x => x != null);

            var copyShaCommand = ReactiveCommand.Create(validCommitObservable)
                .WithSubscription(x => actionMenuService.SendToPasteBoard(this.Commit.Sha));

            var shareCommand = ReactiveCommand.Create(validCommitObservable)
                .WithSubscription(x => actionMenuService.ShareUrl(this.Commit.HtmlUrl));

            var browseCodeCommand = ReactiveCommand.Create(validCommitObservable)
                .WithSubscription(x => 
                {
                    var vm = this.CreateViewModel<SourceTreeViewModel>();
                    vm.RepositoryName = RepositoryName;
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.Branch = this.Commit.Sha;
                    NavigateTo(vm);
                });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                var menu = actionMenuService.Create(Title);
                menu.AddButton("Add Comment", GoToCommentCommand);
                menu.AddButton("Copy SHA", copyShaCommand);
                menu.AddButton("Browse Code", browseCodeCommand);
                menu.AddButton("Share", shareCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                Observable.FromAsync(() => applicationService.GitHubClient.Repository.RepositoryComments.GetForCommit(RepositoryOwner, RepositoryName, Node))
                    .ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => comments.Reset(x));
                Commit = await applicationService.GitHubClient.Repository.Commits.Get(RepositoryOwner, RepositoryName, Node);
            });
        }
    }
}

