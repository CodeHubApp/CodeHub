using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : BaseIssueViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _canMerge;
        public bool CanMerge
        {
            get { return _canMerge.Value; }
        }

        private bool _merged;
        public bool Merged
        {
            get { return _merged; }
            private set { this.RaiseAndSetIfChanged(ref _merged, value); }
        }

        private Octokit.PullRequest _pullRequest;
        public Octokit.PullRequest PullRequest
        { 
            get { return _pullRequest; }
            set { this.RaiseAndSetIfChanged(ref _pullRequest, value); }
        }

        public IReactiveCommand GoToEditCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand GoToCommitsCommand { get; private set; }

        public IReactiveCommand GoToFilesCommand { get; private set; }

        public IReactiveCommand GoToAddCommentCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand MergeCommand { get; private set; }

        public PullRequestViewModel(
            ISessionService applicationService, 
            IMarkdownService markdownService, 
            IActionMenuFactory actionMenuService)
            : base(applicationService, markdownService)
        {
            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Pull Request #" + x);

            _canMerge = this.WhenAnyValue(x => x.PullRequest)
                .Select(x => x != null && !x.Merged)
                .ToProperty(this, x => x.CanMerge);

            var canMergeObservable = this.WhenAnyValue(x => x.PullRequest).Select(x =>
                x != null && !x.Merged && x.Mergeable.HasValue && x.Mergeable.Value);

            MergeCommand = ReactiveCommand.CreateAsyncTask(canMergeObservable, async t =>
            {
                var req = new Octokit.MergePullRequest(null);
                var response = await applicationService.GitHubClient.PullRequest.Merge(RepositoryOwner, RepositoryName, Id, req);
                if (!response.Merged)
                    throw new Exception(string.Format("Unable to merge pull request: {0}", response.Message));
                LoadCommand.ExecuteIfCan();
            });

//            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(
//                this.WhenAnyValue(x => x.PullRequest).Select(x => x != null), 
//                async t =>
//            {
//                var newState = PullRequest.State == Octokit.ItemState.Open ? Octokit.ItemState.Closed : Octokit.ItemState.Open;
//
//                try
//                {
//                    var req = new Octokit.PullRequestUpdate { State = newState };
//                    PullRequest = await applicationService.GitHubClient.PullRequest.Update(RepositoryOwner, RepositoryName, Id, req);
//                }
//                catch (Exception e)
//                {
//                    throw new Exception("Unable to " + (newState == Octokit.ItemState.Closed ? "close" : "open") + " the item. " + e.Message, e);
//                }
//            });


            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.PullRequest).Select(x => x != null));
            GoToHtmlUrlCommand.Select(_ => PullRequest.HtmlUrl).Subscribe(GoToUrlCommand.ExecuteIfCan);

            GoToCommitsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<PullRequestCommitsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.PullRequestId = Id;
                NavigateTo(vm);
            });

            GoToFilesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<PullRequestFilesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.PullRequestId = Id;
                NavigateTo(vm);
            });

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.PullRequest).Select(x => x != null));
            ShareCommand.Subscribe(_ => actionMenuService.ShareUrl(PullRequest.HtmlUrl));

            GoToEditCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
                //vm.Issue = Issue;
//                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                NavigateTo(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.PullRequest).Select(x => x != null), 
                _ =>
            {
                var menu = actionMenuService.Create(Title);
                menu.AddButton("Edit", GoToEditCommand);
                menu.AddButton(PullRequest.State == Octokit.ItemState.Closed ? "Open" : "Close", ToggleStateCommand);
                menu.AddButton("Comment", GoToAddCommentCommand);
                menu.AddButton("Share", ShareCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show();
            });
        }

        protected override async Task Load(ISessionService applicationService)
        {
            PullRequest = await applicationService.GitHubClient.PullRequest.Get(RepositoryOwner, RepositoryName, Id);
            await base.Load(applicationService);
        }
    }
}
