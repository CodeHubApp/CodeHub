using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using System.Reactive;
using System.Threading;
using System.Collections.Generic;
using CodeHub.Core.Factories;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        private int _id;
        public int Id 
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

        private string _repositoryOwner;
        public string RepositoryOwner
        {
            get { return _repositoryOwner; }
            set { this.RaiseAndSetIfChanged(ref _repositoryOwner, value); }
        }

        private string _repositoryName;
        public string RepositoryName
        {
            get { return _repositoryName; }
            set { this.RaiseAndSetIfChanged(ref _repositoryName, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _markdownDescription;
		public string MarkdownDescription
		{
            get { return _markdownDescription.Value; }
		}

        private readonly ObservableAsPropertyHelper<Octokit.User> _assignedUser;
        public Octokit.User AssignedUser
        {
            get { return _assignedUser.Value; }
        }

        private readonly ObservableAsPropertyHelper<Octokit.Milestone> _assignedMilestone;
        public Octokit.Milestone AssignedMilestone
        {
            get { return _assignedMilestone.Value; }
        }

        private readonly ObservableAsPropertyHelper<ICollection<Octokit.Label>> _assignedLabels;
        public ICollection<Octokit.Label> AssignedLabels
        {
            get { return _assignedLabels.Value; }
        }

        private Octokit.Issue _issueModel;
        public Octokit.Issue Issue
        {
            get { return _issueModel; }
            set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        public IssueAssigneeViewModel Assignees { get; private set; }

        public IssueLabelsViewModel Labels { get; private set; }

        public IssueMilestonesViewModel Milestones { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToAssigneesCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestonesCommand { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

		public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand<Unit> ToggleStateCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<object> AddCommentCommand { get; private set; }

        public IReadOnlyReactiveList<IIssueEventItemViewModel> Events { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public IssueViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuFactory,
            IShareService shareService, IMarkdownService markdownService, IGraphicService graphicsService)
        {
            _applicationService = applicationService;

            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue).Select(x => x != null);

            GoToAssigneesCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Assignees.LoadCommand.ExecuteIfCan());

            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Labels.LoadCommand.ExecuteIfCan());

            GoToMilestonesCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Milestones.LoadCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Issue #" + x);

            _assignedUser = this.WhenAnyValue(x => x.Issue.Assignee)
                .ToProperty(this, x => x.AssignedUser);

            _assignedMilestone = this.WhenAnyValue(x => x.Issue.Milestone)
                .ToProperty(this, x => x.AssignedMilestone);

            _assignedLabels = this.WhenAnyValue(x => x.Issue.Labels)
                .ToProperty(this, x => x.AssignedLabels);

            _markdownDescription = this.WhenAnyValue(x => x.Issue)
                .Select(x => ((x == null || string.IsNullOrEmpty(x.Body)) ? null : markdownService.Convert(x.Body)))
                .ToProperty(this, x => x.MarkdownDescription);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Issue.HtmlUrl));

            var events = new ReactiveList<IIssueEventItemViewModel>();
            Events = events.CreateDerivedCollection(x => x);

            AddCommentCommand = ReactiveCommand.Create();
            AddCommentCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueCommentViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
                vm.SaveCommand.Subscribe(x => events.Add(new IssueCommentItemViewModel(x)));
                NavigateTo(vm);
            });

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(issuePresenceObservable, async t =>
            {
                try
                {
                    Issue = await applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, new Octokit.IssueUpdate {
                        State = (Issue.State == Octokit.ItemState.Open) ? Octokit.ItemState.Closed : Octokit.ItemState.Open
                    });
                }
                catch (Exception e)
                {
                    var close = (Issue.State == Octokit.ItemState.Open) ? "close" : "open";
                    throw new Exception("Unable to " + close + " the item. " + e.Message, e);
                }
            });

            GoToEditCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToEditCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
//                vm.Issue = Issue;
//                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                NavigateTo(vm);
            });

            Assignees = new IssueAssigneeViewModel(
                () => applicationService.GitHubClient.Issue.Assignee.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult(Issue),
                UpdateIssue);

            Milestones = new IssueMilestonesViewModel(
                () => applicationService.GitHubClient.Issue.Milestone.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult(Issue),
                UpdateIssue);

            Labels = new IssueLabelsViewModel(
                () => applicationService.GitHubClient.Issue.Labels.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult(Issue),
                UpdateIssue,
                graphicsService);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var issueRequest = applicationService.GitHubClient.Issue.Get(RepositoryOwner, RepositoryName, Id)
                    .ContinueWith(x => Issue = x.Result, new CancellationToken(), TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
                var eventsRequest = applicationService.GitHubClient.Issue.Events.GetForIssue(RepositoryOwner, RepositoryName, Id);
                var commentsRequest = applicationService.GitHubClient.Issue.Comment.GetForIssue(RepositoryOwner, RepositoryName, Id);
                await Task.WhenAll(issueRequest, eventsRequest, commentsRequest);

                var tempList = new List<IIssueEventItemViewModel>(eventsRequest.Result.Count + commentsRequest.Result.Count);
                tempList.AddRange(eventsRequest.Result.Select(x => new IssueEventItemViewModel(x)));
                tempList.AddRange(commentsRequest.Result.Select(x => new IssueCommentItemViewModel(x)));
                events.Reset(tempList.OrderBy(x => x.CreatedAt));
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                _ =>
            {
                var menu = actionMenuFactory.Create(Title);
                menu.AddButton(Issue.State == Octokit.ItemState.Open ? "Close" : "Open", ToggleStateCommand);
//
//
//                var editButton = _actionSheet.AddButton("Edit");
//                var commentButton = _actionSheet.AddButton("Comment");
//                var shareButton = _actionSheet.AddButton("Share");
//                var showButton = _actionSheet.AddButton("Show in GitHub");

                return menu.Show();
            });
        }

        private async Task<Octokit.Issue> UpdateIssue(Octokit.IssueUpdate update)
        {
            return Issue = await _applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, update);
        }
    }
}

