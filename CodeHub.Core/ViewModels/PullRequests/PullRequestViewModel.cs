using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;
using CodeHub.Core.Factories;
using System.Reactive;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : BaseIssueViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _canMerge;
        public bool CanMerge
        {
            get { return _canMerge.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> _merged;
        public bool Merged
        {
            get { return _merged.Value; }
        }

        private Octokit.PullRequest _pullRequest;
        public Octokit.PullRequest PullRequest
        { 
            get { return _pullRequest; }
            private set { this.RaiseAndSetIfChanged(ref _pullRequest, value); }
        }

        private string _mergeComment;
        public string MergeComment 
        {
            get { return _mergeComment; }
            set { this.RaiseAndSetIfChanged(ref _mergeComment, value); }
        }

        private bool _pushAccess;
        public bool PushAccess
        { 
            get { return _pushAccess; }
            private set { this.RaiseAndSetIfChanged(ref _pushAccess, value); }
        }

        private readonly ObservableAsPropertyHelper<Uri> _htmlUrl;
        protected override Uri HtmlUrl
        {
            get { return _htmlUrl.Value; }
        }

        public IReactiveCommand GoToCommitsCommand { get; private set; }

        public IReactiveCommand GoToFilesCommand { get; private set; }

        public IReactiveCommand<Unit> MergeCommand { get; private set; }

        public PullRequestViewModel(
            ISessionService applicationService, 
            IMarkdownService markdownService, 
            IActionMenuFactory actionMenuService,
            IAlertDialogFactory alertDialogFactory)
            : base(applicationService, markdownService, actionMenuService)
        {
            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Pull Request #" + x);

            _merged = this.WhenAnyValue(x => x.PullRequest.Merged)
                .ToProperty(this, x => x.Merged);

            _htmlUrl = this.WhenAnyValue(x => x.PullRequest.HtmlUrl)
                .ToProperty(this, x => x.HtmlUrl);

            var canMergeObservable = this.WhenAnyValue(x => x.PullRequest)
                .Select(x => x != null && !x.Merged && x.Mergeable.HasValue && x.Mergeable.Value);

            _canMerge = canMergeObservable.CombineLatest(
                this.WhenAnyValue(x => x.PushAccess), (x, y) => x && y)
                .ToProperty(this, x => x.CanMerge);

            MergeCommand = ReactiveCommand.CreateAsyncTask(canMergeObservable, async t =>  {
                var req = new Octokit.MergePullRequest(MergeComment ?? string.Empty);

                using (alertDialogFactory.Activate("Merging..."))
                {
                    var response = await applicationService.GitHubClient.PullRequest.Merge(RepositoryOwner, RepositoryName, Id, req);
                    if (!response.Merged)
                        throw new Exception(string.Format("Unable to merge pull request: {0}", response.Message));
                    await LoadCommand.ExecuteAsync();
                }
            });

            GoToCommitsCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<PullRequestCommitsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.PullRequestId = Id;
                NavigateTo(vm);
            });

            GoToFilesCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<PullRequestFilesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.PullRequestId = Id;
                NavigateTo(vm);
            });
        }

        public PullRequestViewModel Init(string repositoryOwner, string repositoryName, int id, Octokit.PullRequest pullRequest = null)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Id = id;
            PullRequest = pullRequest;
            return this;
        }

        protected override async Task Load(ISessionService applicationService)
        {
            PullRequest = await applicationService.GitHubClient.PullRequest.Get(RepositoryOwner, RepositoryName, Id);

            applicationService.GitHubClient.Repository.Get(RepositoryOwner, RepositoryName)
                .ToBackground(x => PushAccess = x.Permissions.Push);

            await base.Load(applicationService);
        }

        protected override async Task<IEnumerable<IIssueEventItemViewModel>> RetrieveEvents()
        {
            var events = (await base.RetrieveEvents()).ToList();
            if (PullRequest == null)
                return events;

            try
            {
                events.Insert(0, CreateInitialComment(PullRequest));
            }
            catch
            {
                Debugger.Break();
            }

            return events;
        }

        private static IssueCommentItemViewModel CreateInitialComment(Octokit.PullRequest pullRequest)
        {
            var login = pullRequest.User.Login;
            var loginHtml = pullRequest.User.HtmlUrl;
            var baseHtml = pullRequest.Base.Repository.HtmlUrl + "/tree/" + pullRequest.Base.Ref;
            var headHtml = pullRequest.Head.Repository.HtmlUrl + "/tree/" + pullRequest.Head.Ref;

            var body = "<a href='" + loginHtml + "'>" + login + "</a> wants to merge " + pullRequest.Commits + " commit" +
                (pullRequest.Commits > 1 ? "s" : string.Empty) + " into <a href='" + baseHtml + "'>" + pullRequest.Base.Label + 
                "</a> from <a href='" + headHtml + "'>" + pullRequest.Head.Label + "</a>";

            return new IssueCommentItemViewModel(body, login, pullRequest.User.AvatarUrl, pullRequest.CreatedAt);
        }
    }
}
