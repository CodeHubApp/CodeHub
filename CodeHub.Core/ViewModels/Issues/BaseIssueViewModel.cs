using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Reactive;
using System.Linq;
using CodeHub.Core.ViewModels.Users;
using System.Collections.ObjectModel;
using CodeHub.Core.Factories;

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
            protected set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

        private string _repositoryOwner;
        public string RepositoryOwner
        {
            get { return _repositoryOwner; }
            protected set { this.RaiseAndSetIfChanged(ref _repositoryOwner, value); }
        }

        private string _repositoryName;
        public string RepositoryName
        {
            get { return _repositoryName; }
            protected set { this.RaiseAndSetIfChanged(ref _repositoryName, value); }
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

        protected abstract Uri HtmlUrl { get; }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

        public IssueAssigneeViewModel Assignees { get; private set; }

        public IssueLabelsViewModel Labels { get; private set; }

        public IssueMilestonesViewModel Milestones { get; private set; }

        public IReactiveCommand<object> GoToAssigneesCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestonesCommand { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public IReadOnlyReactiveList<IIssueEventItemViewModel> Events { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> ToggleStateCommand { get; private set; }

        public IReactiveCommand<object> AddCommentCommand { get; private set; }

        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        protected BaseIssueViewModel(
            ISessionService applicationService, 
            IMarkdownService markdownService,
            IActionMenuFactory actionMenuFactory)
        {
            _applicationService = applicationService;

            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue, x => x.CanModify)
                .Select(x => x.Item1 != null && x.Item2);
            Events = InternalEvents.CreateDerivedCollection(x => x);

            _participants = Events.Changed
                .Select(_ => Events.Select(y => y.Actor).Distinct().Count())
                .Select(x => x == 0 ? 1 : x)
                .ToProperty(this, x => x.Participants);

            GoToAssigneesCommand = ReactiveCommand.Create(issuePresenceObservable);
                //.WithSubscription(_ => Assignees.LoadCommand.ExecuteIfCan());

            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable);
                //.WithSubscription(_ => Labels.LoadCommand.ExecuteIfCan());

            GoToMilestonesCommand = ReactiveCommand.Create(issuePresenceObservable);
                //.WithSubscription(_ => Milestones.LoadCommand.ExecuteIfCan());

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
            GoToOwnerCommand
                .Select(_ => this.CreateViewModel<UserViewModel>())
                .Select(x => x.Init(Issue.User.Login))
                .Subscribe(NavigateTo);

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

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                sender => {
                    var menu = actionMenuFactory.Create(Title);
                    menu.AddButton("Edit", GoToEditCommand);
                    menu.AddButton(Issue.State == Octokit.ItemState.Closed ? "Open" : "Close", ToggleStateCommand);
                    menu.AddButton("Comment", AddCommentCommand);
                    menu.AddButton("Share", ShareCommand);
                    menu.AddButton("Show in GitHub", GoToHtmlUrlCommand); 
                    return menu.Show(sender);
                });

            GoToEditCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
                //vm.Issue = Issue;
                //                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                NavigateTo(vm);
            });

            GoToUrlCommand = ReactiveCommand.Create();
            GoToUrlCommand.OfType<string>().Subscribe(GoToUrl);
            GoToUrlCommand.OfType<Uri>().Subscribe(x => GoToUrl(x.AbsoluteUri));

            var hasHtmlObservable = this.WhenAnyValue(x => x.HtmlUrl).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(hasHtmlObservable);
            ShareCommand.Subscribe(_ => actionMenuFactory.ShareUrl(HtmlUrl));

            GoToHtmlUrlCommand = ReactiveCommand.Create(hasHtmlObservable);
            GoToHtmlUrlCommand.Subscribe(_ => GoToUrl(HtmlUrl.AbsoluteUri));
        }

        private void GoToUrl(string url)
        {
            var vm = this.CreateViewModel<WebBrowserViewModel>();
            vm.Init(url);
            NavigateTo(vm);
        }

        protected virtual async Task Load(ISessionService applicationService)
        {
            var issueRequest = applicationService.GitHubClient.Issue.Get(RepositoryOwner, RepositoryName, Id);
            var eventsRequest = RetrieveEvents();

            await Task.WhenAll(issueRequest, eventsRequest);

            Issue = issueRequest.Result;
            InternalEvents.Reset(eventsRequest.Result);

            applicationService.GitHubClient.Repository.RepoCollaborators
                .IsCollaborator(RepositoryOwner, RepositoryName, applicationService.Account.Username)
                .ToBackground(x => CanModify = x);
        }

        protected virtual async Task<IEnumerable<IIssueEventItemViewModel>> RetrieveEvents()
        {
            var eventsRequest = _applicationService.GitHubClient.Issue.Events.GetForIssue(RepositoryOwner, RepositoryName, Id);
            var commentsRequest = _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].GetComments());
            await Task.WhenAll(eventsRequest, commentsRequest);

            var tempList = new List<IIssueEventItemViewModel>(eventsRequest.Result.Count + commentsRequest.Result.Data.Count);
            tempList.AddRange(eventsRequest.Result.Select(x => new IssueEventItemViewModel(x)));
            tempList.AddRange(commentsRequest.Result.Data.Select(x => new IssueCommentItemViewModel(x)));
            return tempList.OrderBy(x => x.CreatedAt);
        }

        private async Task<Octokit.Issue> UpdateIssue(Octokit.IssueUpdate update)
        {
            Issue = await _applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, update);
            InternalEvents.Reset(await RetrieveEvents());
            return Issue;
        }
    }
}

