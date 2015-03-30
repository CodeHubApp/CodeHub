using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitViewModel : BaseViewModel, ILoadableViewModel
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

        private readonly ObservableAsPropertyHelper<string> _commitMessageSummary;
        public string CommitMessageSummary
        {
            get { return _commitMessageSummary.Value; }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFileCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand AddCommentCommand { get; private set; }

        public IReadOnlyReactiveList<CommitCommentItemViewModel> Comments { get; private set; }

        public IReactiveCommand GoToUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }
        
        public CommitViewModel(ISessionService applicationService, IActionMenuFactory actionMenuService)
        {
            Title = "Commit";

            var comments = new ReactiveList<GitHubSharp.Models.CommentModel>();
            Comments = comments.CreateDerivedCollection(x => new CommitCommentItemViewModel(x));

            this.WhenAnyValue(x => x.Commit)
                .IsNotNull()
                .Select(x => x.Commit.Message ?? string.Empty)
                .Select(x =>
                {
                    var firstNewLine = x.IndexOf("\n", StringComparison.Ordinal);
                    return firstNewLine > 0 ? x.Substring(0, firstNewLine) : x;
                })
                .ToProperty(this, x => x.CommitMessageSummary, out _commitMessageSummary);

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Commit).Select(x => x != null));
            GoToHtmlUrlCommand
                .Select(_ => this.CreateViewModel<WebBrowserViewModel>())
                .Select(x => x.Init(Commit.HtmlUrl))
                .Subscribe(NavigateTo);
      
            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                NavigateTo(vm);
            });

            AddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
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
                    vm.Path = x.Filename;
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Name = System.IO.Path.GetFileName(x.Filename);
                    vm.ForceBinary = true;
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

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(sender => {
                var menu = actionMenuService.Create();
                menu.AddButton("Add Comment", AddCommentCommand);
                menu.AddButton("Copy SHA", copyShaCommand);
                menu.AddButton("Browse Code", browseCodeCommand);
                menu.AddButton("Share", shareCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show(sender);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var commentRequest = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.GetAll();
                applicationService.Client.ExecuteAsync(commentRequest).ToBackground(x => comments.Reset(x.Data));
                Commit = await applicationService.GitHubClient.Repository.Commits.Get(RepositoryOwner, RepositoryName, Node);
            });
        }
    }
}

