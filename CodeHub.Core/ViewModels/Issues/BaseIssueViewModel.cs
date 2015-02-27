using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Reactive;
using System.Threading;
using System.Linq;
using CodeHub.Core.ViewModels.Users;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssueViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly ReactiveList<IIssueEventItemViewModel> InternalEvents = new ReactiveList<IIssueEventItemViewModel>();
        private readonly ISessionService _applicationService;

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

        private readonly ObservableAsPropertyHelper<IReadOnlyList<Octokit.Label>> _assignedLabels;
        public IReadOnlyList<Octokit.Label> AssignedLabels
        {
            get { return _assignedLabels.Value; }
        }

        private readonly ObservableAsPropertyHelper<int> _participants;
        public int Participants
        {
            get { return _participants.Value; }
        }

        private Octokit.Issue _issueModel;
        public Octokit.Issue Issue
        {
            get { return _issueModel; }
            set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _markdownDescription;
        public string MarkdownDescription
        {
            get { return _markdownDescription.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> _isClosed;
        public bool IsClosed
        {
            get { return _isClosed.Value; }
        }

        private bool _canModify;
        public bool CanModify
        {
            get { return _canModify; }
            private set { this.RaiseAndSetIfChanged(ref _canModify, value); }
        }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

        public IssueAssigneeViewModel Assignees { get; private set; }

        public IssueLabelsViewModel Labels { get; private set; }

        public IssueMilestonesViewModel Milestones { get; private set; }

        public IReactiveCommand<object> GoToAssigneesCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestonesCommand { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; protected set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public IReadOnlyReactiveList<IIssueEventItemViewModel> Events { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> ToggleStateCommand { get; private set; }

        public IReactiveCommand<object> AddCommentCommand { get; private set; }

        protected BaseIssueViewModel(
            ISessionService applicationService, 
            IMarkdownService markdownService)
        {
            _applicationService = applicationService;

            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue, x => x.CanModify)
                .Select(x => x.Item1 != null && x.Item2);
            Events = InternalEvents.CreateDerivedCollection(x => x);

            _participants = Events.Changed
                .Select(_ => Events.Select(y => y.Actor).Distinct().Count())
                .Select(x => x == 0 ? 1 : x)
                .ToProperty(this, x => x.Participants);

            GoToAssigneesCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Assignees.LoadCommand.ExecuteIfCan());

            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Labels.LoadCommand.ExecuteIfCan());

            GoToMilestonesCommand = ReactiveCommand.Create(issuePresenceObservable)
                .WithSubscription(_ => Milestones.LoadCommand.ExecuteIfCan());

            _assignedUser = this.WhenAnyValue(x => x.Issue.Assignee)
                .ToProperty(this, x => x.AssignedUser);

            _assignedMilestone = this.WhenAnyValue(x => x.Issue.Milestone)
                .ToProperty(this, x => x.AssignedMilestone);

            _assignedLabels = this.WhenAnyValue(x => x.Issue.Labels)
                .ToProperty(this, x => x.AssignedLabels);

            _isClosed = this.WhenAnyValue(x => x.Issue.State)
                .Select(x => x == Octokit.ItemState.Closed)
                .ToProperty(this, x => x.IsClosed);

            Assignees = new IssueAssigneeViewModel(
                () => applicationService.GitHubClient.Issue.Assignee.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult(Issue.Assignee),
                x => UpdateIssue(new Octokit.IssueUpdate 
                { 
                    Assignee = x.With(y => y.Login),
                    Milestone = AssignedMilestone.With(y => (int?)y.Number)
                }));

            Milestones = new IssueMilestonesViewModel(
                () => applicationService.GitHubClient.Issue.Milestone.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult(Issue.Milestone),
                x => UpdateIssue(new Octokit.IssueUpdate 
                { 
                    Assignee = AssignedUser.With(y => y.Login),
                    Milestone = x.With(y => (int?)y.Number)
                }));

            Labels = new IssueLabelsViewModel(
                () => applicationService.GitHubClient.Issue.Labels.GetForRepository(RepositoryOwner, RepositoryName),
                () => Task.FromResult((IReadOnlyList<Octokit.Label>)new ReadOnlyCollection<Octokit.Label>(Issue.Labels.ToList())),
                x =>
                {
                    var update = new Octokit.IssueUpdate
                    { 
                        Assignee = AssignedUser.With(y => y.Login),
                        Milestone = AssignedMilestone.With(y => (int?)y.Number),
                    };

                    foreach (var label in x.Select(y => y.Name))
                        update.AddLabel(label);
                    return UpdateIssue(update);
                });

            _markdownDescription = this.WhenAnyValue(x => x.Issue)
                .Select(x => ((x == null || string.IsNullOrEmpty(x.Body)) ? null : markdownService.Convert(x.Body)))
                .ToProperty(this, x => x.MarkdownDescription);

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService));

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            GoToOwnerCommand.Select(_ => Issue.User).Subscribe(x =>
            {
                var vm = this.CreateViewModel<UserViewModel>();
                vm.Username = x.Login;
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

            AddCommentCommand = ReactiveCommand.Create()
                .WithSubscription(_ =>
                {
                    var vm = this.CreateViewModel<IssueCommentViewModel>();
                    vm.RepositoryOwner = RepositoryOwner;
                    vm.RepositoryName = RepositoryName;
                    vm.Id = Id;
                    vm.SaveCommand.Subscribe(x => InternalEvents.Add(new IssueCommentItemViewModel(x)));
                    NavigateTo(vm);
                });


            GoToUrlCommand = ReactiveCommand.Create();
            GoToUrlCommand.OfType<string>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });
            GoToUrlCommand.OfType<Uri>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x.AbsoluteUri;
                NavigateTo(vm);
            });
        }

        protected virtual async Task Load(ISessionService applicationService)
        {
            var issueRequest = applicationService.GitHubClient.Issue.Get(RepositoryOwner, RepositoryName, Id);
            var eventsRequest = RetrieveEvents();

            await Task.WhenAll(issueRequest, eventsRequest);

            Issue = issueRequest.Result;

            applicationService.GitHubClient.Repository.RepoCollaborators
                .IsCollaborator(RepositoryOwner, RepositoryName, applicationService.Account.Username)
                .ToBackground(x => CanModify = x);
        }

        private async Task RetrieveEvents()
        {
            var eventsRequest = _applicationService.GitHubClient.Issue.Events.GetForIssue(RepositoryOwner, RepositoryName, Id);
            var commentsRequest = _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].GetComments());
            await Task.WhenAll(eventsRequest, commentsRequest);

            var tempList = new List<IIssueEventItemViewModel>(eventsRequest.Result.Count + commentsRequest.Result.Data.Count);
            tempList.AddRange(eventsRequest.Result.Select(x => new IssueEventItemViewModel(x)));
            tempList.AddRange(commentsRequest.Result.Data.Select(x => new IssueCommentItemViewModel(x)));
            InternalEvents.Reset(tempList.OrderBy(x => x.CreatedAt));
        }

        private async Task<Octokit.Issue> UpdateIssue(Octokit.IssueUpdate update)
        {
            Issue = await _applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, update);
            await RetrieveEvents();
            return Issue;
        }
    }
}

