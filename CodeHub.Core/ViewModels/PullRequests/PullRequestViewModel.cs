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
using Octokit;
using System.Collections.ObjectModel;
using Splat;
using System.Text;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : BaseIssueViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _canMerge;
        public bool CanMerge
        {
            get { return _canMerge.Value; }
        }

        private PullRequest _pullRequest;
        public PullRequest PullRequest
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

        private IReadOnlyList<PullRequestReviewComment> _comments;
        public IReadOnlyList<PullRequestReviewComment> Comments
        {
            get { return _comments; }
            set { this.RaiseAndSetIfChanged(ref _comments, value); }
        }

        public IReactiveCommand<object> GoToCommitsCommand { get; private set; }

        public IReactiveCommand<object> GoToFilesCommand { get; private set; }

        public IReactiveCommand<Unit> MergeCommand { get; private set; }

        public PullRequestViewModel(
            ISessionService applicationService, 
            IMarkdownService markdownService, 
            IActionMenuFactory actionMenuService,
            IAlertDialogFactory alertDialogFactory)
            : base(applicationService, markdownService, actionMenuService, alertDialogFactory)
        {
            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Pull Request #" + x);

            this.WhenAnyValue(x => x.PullRequest.HtmlUrl)
                .ToProperty(this, x => x.HtmlUrl, out _htmlUrl);

            var canMergeObservable = this.WhenAnyValue(x => x.PullRequest)
                .Select(x => x != null && !x.Merged && x.Mergeable.HasValue && x.Mergeable.Value);

            _canMerge = canMergeObservable.CombineLatest(
                this.WhenAnyValue(x => x.PushAccess), (x, y) => x && y)
                .ToProperty(this, x => x.CanMerge);

            _commentsCount = this.WhenAnyValue(x => x.Issue.Comments, x => x.Comments.Count, (x, y) => x + y)
                .ToProperty(this, x => x.CommentCount);

            MergeCommand = ReactiveCommand.CreateAsyncTask(canMergeObservable, async t =>  {
                using (alertDialogFactory.Activate("Merging..."))
                {
                    var req = new MergePullRequest { Message = MergeComment };
                    var response = await applicationService.GitHubClient.PullRequest.Merge(RepositoryOwner, RepositoryName, Id, req);
                    if (!response.Merged)
                        throw new Exception(string.Format("Unable to merge pull request: {0}", response.Message));
                    await LoadCommand.ExecuteAsync();
                }
            });

            GoToCommitsCommand = ReactiveCommand.Create();
            GoToCommitsCommand
                .Select(x => this.CreateViewModel<PullRequestCommitsViewModel>())
                .Select(x => x.Init(RepositoryOwner, RepositoryName, Id))
                .Subscribe(NavigateTo);

            var canGoToFiles = this.WhenAnyValue(x => x.PullRequest).Select(x => x != null);
            GoToFilesCommand = ReactiveCommand.Create(canGoToFiles);
            GoToFilesCommand
                .Select(x => this.CreateViewModel<PullRequestFilesViewModel>())
                .Select(x => x.Init(RepositoryOwner, RepositoryName, Id, PullRequest.Head.Sha))
                .Do(x => x.CommentCreated.Subscribe(AddComment))
                .Subscribe(NavigateTo);
        }

        private void AddComment(PullRequestReviewComment reviewComment)
        {
            var comments = _comments.ToList();
            comments.Add(reviewComment);
            Comments = new ReadOnlyCollection<PullRequestReviewComment>(comments);
        }

        public PullRequestViewModel Init(string repositoryOwner, string repositoryName, int id, PullRequest pullRequest = null, Issue issue = null)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Id = id;
            PullRequest = pullRequest;
            Issue = issue;
            return this;
        }

        protected override async Task Load(ISessionService applicationService)
        {
            PullRequest = await applicationService.GitHubClient.PullRequest.Get(RepositoryOwner, RepositoryName, Id);

            await base.Load(applicationService);

            applicationService.GitHubClient.Repository.PullRequest.Comment.GetAll(RepositoryOwner, RepositoryName, Id)
                .ToBackground(x => Comments = x);

            applicationService.GitHubClient.Repository.Get(RepositoryOwner, RepositoryName)
                .ToBackground(x => PushAccess = x.Permissions.Push);
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
            catch (Exception e)
            {
                this.Log().ErrorException("Unable to load events", e);
            }

            return events;
        }

        private static IssueCommentItemViewModel CreateInitialComment(PullRequest pullRequest)
        {
            var str = new StringBuilder();
            var login = pullRequest.User.Login;
            var loginHtml = pullRequest.User.HtmlUrl;
            str.AppendFormat("<a href='{0}'>{1}</a> wants to merge ", loginHtml, login);
            str.AppendFormat("{0} commit{1} ", pullRequest.Commits, (pullRequest.Commits > 1 ? "s" : string.Empty));
            str.AppendFormat("{0} commit{1} into ", pullRequest.Commits, (pullRequest.Commits > 1 ? "s" : string.Empty));
            str.Append(CreateLink(pullRequest.Base));
            str.Append(" from ");
            str.Append(CreateLink(pullRequest.Head));
            return new IssueCommentItemViewModel(str.ToString(), login, pullRequest.User.AvatarUrl, pullRequest.CreatedAt);
        }

        private static string CreateLink(GitReference reference)
        {
            if (reference.Repository != null)
            {
                var baseHtml = reference.Repository.HtmlUrl + "/tree/" + reference.Ref;
                return string.Format("<a href='{0}'>{1}</a>", baseHtml, reference.Label);
            }

            return "<a href='#'>unknown repository</a>";
        }
    }
}
