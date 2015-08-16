using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Reactive;
using System.Linq;
using CodeHub.Core.ViewModels.Users;
using CodeHub.Core.Factories;
using Splat;
using System.Reactive.Subjects;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class BaseIssueViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly ReactiveList<IIssueEventItemViewModel> InternalEvents = new ReactiveList<IIssueEventItemViewModel>();
        private readonly ISubject<Octokit.Issue> _issueUpdatedObservable = new Subject<Octokit.Issue>();
        private readonly ISessionService _applicationService;
        private readonly IMarkdownService _markdownService;
        private readonly IAlertDialogFactory _alertDialogFactory;
        private readonly Lazy<Task<IReadOnlyList<Octokit.User>>> _assigneesCache;
        private readonly Lazy<Task<IReadOnlyList<Octokit.Milestone>>> _milestonesCache;
        private readonly Lazy<Task<IReadOnlyList<Octokit.Label>>> _labelsCache;

        public IObservable<Octokit.Issue> IssueUpdated
        {
            get { return _issueUpdatedObservable.AsObservable(); }
        }

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

        protected ObservableAsPropertyHelper<int> _commentsCount;
        public int CommentCount
        {
            get { return _commentsCount.Value; }
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
            protected set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
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

        private readonly ObservableAsPropertyHelper<GitHubAvatar> _avatar;
        public GitHubAvatar Avatar
        {
            get { return _avatar.Value; }
        }

        protected abstract Uri HtmlUrl { get; }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

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
            IActionMenuFactory actionMenuFactory,
            IAlertDialogFactory alertDialogFactory)
        {
            _applicationService = applicationService;
            _markdownService = markdownService;
            _alertDialogFactory = alertDialogFactory;

            _assigneesCache = new Lazy<Task<IReadOnlyList<Octokit.User>>>(() => 
                _applicationService.GitHubClient.Issue.Assignee.GetAllForRepository(RepositoryOwner, RepositoryName));
            _milestonesCache = new Lazy<Task<IReadOnlyList<Octokit.Milestone>>>(() => 
                _applicationService.GitHubClient.Issue.Milestone.GetAllForRepository(RepositoryOwner, RepositoryName));
            _labelsCache = new Lazy<Task<IReadOnlyList<Octokit.Label>>>(() => 
                _applicationService.GitHubClient.Issue.Labels.GetAllForRepository(RepositoryOwner, RepositoryName));
            
            IssueUpdated.Subscribe(x => Issue = x);

            _avatar = this.WhenAnyValue(x => x.Issue.User.AvatarUrl)
                .Select(x => new GitHubAvatar(x))
                .ToProperty(this, x => x.Avatar);

            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue, x => x.CanModify)
                .Select(x => x.Item1 != null && x.Item2);
            Events = InternalEvents.CreateDerivedCollection(x => x);

            this.WhenAnyValue(x => x.Issue.Comments)
                .ToProperty(this, x => x.CommentCount, out _commentsCount);

            _participants = Events.Changed
                .Select(_ => Events.Select(y => y.Actor).Distinct().Count())
                .Select(x => x == 0 ? 1 : x)
                .ToProperty(this, x => x.Participants);

            GoToAssigneesCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToMilestonesCommand = ReactiveCommand.Create(issuePresenceObservable);

            _assignedUser = this.WhenAnyValue(x => x.Issue.Assignee)
                .ToProperty(this, x => x.AssignedUser);

            _assignedMilestone = this.WhenAnyValue(x => x.Issue.Milestone)
                .ToProperty(this, x => x.AssignedMilestone);

            _assignedLabels = this.WhenAnyValue(x => x.Issue.Labels)
                .ToProperty(this, x => x.AssignedLabels);

            _isClosed = this.WhenAnyValue(x => x.Issue.State)
                .Select(x => x == Octokit.ItemState.Closed)
                .ToProperty(this, x => x.IsClosed);

            _markdownDescription = this.WhenAnyValue(x => x.Issue)
                .Select(x => ((x == null || string.IsNullOrEmpty(x.Body)) ? null : x.Body))
                .Where(x => x != null)
                .Select(x => GetMarkdownDescription().ToObservable())
                .Switch()
                .ToProperty(this, x => x.MarkdownDescription, null, RxApp.MainThreadScheduler);

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => Load(applicationService));

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            GoToOwnerCommand
                .Select(_ => this.CreateViewModel<UserViewModel>())
                .Select(x => x.Init(Issue.User.Login))
                .Subscribe(NavigateTo);

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(issuePresenceObservable, async t => {
                try
                {
                    var updatedIssue = await applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, new Octokit.IssueUpdate {
                        State = (Issue.State == Octokit.ItemState.Open) ? Octokit.ItemState.Closed : Octokit.ItemState.Open
                    });
                    _issueUpdatedObservable.OnNext(updatedIssue);
                }
                catch (Exception e)
                {
                    var close = (Issue.State == Octokit.ItemState.Open) ? "close" : "open";
                    throw new Exception("Unable to " + close + " the item. " + e.Message, e);
                }

                RetrieveEvents().ToBackground(x => InternalEvents.Reset(x));
            });

            AddCommentCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = new ComposerViewModel(async s => {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].CreateComment(s);
                    var comment = (await applicationService.Client.ExecuteAsync(request)).Data;
                    InternalEvents.Add(new IssueCommentItemViewModel(comment));
                }, alertDialogFactory);
                NavigateTo(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                sender => {
                    var menu = actionMenuFactory.Create();
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
                vm.Issue = Issue;
                vm.SaveCommand.Subscribe(_issueUpdatedObservable.OnNext);
                NavigateTo(vm);
            });

            GoToUrlCommand = ReactiveCommand.Create();
            GoToUrlCommand.OfType<string>().Subscribe(GoToUrl);
            GoToUrlCommand.OfType<Uri>().Subscribe(x => GoToUrl(x.AbsoluteUri));

            var hasHtmlObservable = this.WhenAnyValue(x => x.HtmlUrl).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(hasHtmlObservable);
            ShareCommand.Subscribe(sender => actionMenuFactory.ShareUrl(sender, HtmlUrl));

            GoToHtmlUrlCommand = ReactiveCommand.Create(hasHtmlObservable);
            GoToHtmlUrlCommand.Subscribe(_ => GoToUrl(HtmlUrl.AbsoluteUri));
        }

        private async Task<string> GetMarkdownDescription()
        {
            try
            {
                var connection = _applicationService.GitHubClient.Connection;
                var ret = await connection.Post<string>(new Uri("/markdown", UriKind.Relative), new {
                    text = Issue.Body,
                    mode = "gfm",
                    context = string.Format("{0}/{1}", RepositoryOwner, RepositoryName)
                }, "application/json", "application/json");
                return ret.Body;
            }
            catch (Exception e)
            {
                this.Log().InfoException("Unable to retrieve GitHub Markdown. Falling back to local", e);
                return _markdownService.Convert(Issue.Body);
            }
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

        public IssueAssigneeViewModel CreateAssigneeViewModel()
        {
            var vm = new IssueAssigneeViewModel(
                () => _assigneesCache.Value,
                () => Task.FromResult(AssignedUser),
                x => UpdateIssue(x, AssignedMilestone, AssignedLabels));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        public IssueMilestonesViewModel CreateMilestonesViewModel()
        {
            var vm = new IssueMilestonesViewModel(
                () => _milestonesCache.Value,
                () => Task.FromResult(AssignedMilestone),
                x => UpdateIssue(AssignedUser, x, AssignedLabels));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        public IssueLabelsViewModel CreateLabelsViewModel()
        {
            var vm = new IssueLabelsViewModel(
                () => _labelsCache.Value,
                () => Task.FromResult(AssignedLabels),
                x => UpdateIssue(AssignedUser, AssignedMilestone, x));
            vm.LoadCommand.ExecuteIfCan();
            return vm;
        }

        protected virtual async Task<IEnumerable<IIssueEventItemViewModel>> RetrieveEvents()
        {
            var eventsRequest = _applicationService.GitHubClient.Issue.Events.GetAllForIssue(RepositoryOwner, RepositoryName, Id);
            var commentsRequest = _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].GetComments());
            await Task.WhenAll(eventsRequest, commentsRequest);

            var tempList = new List<IIssueEventItemViewModel>(eventsRequest.Result.Count + commentsRequest.Result.Data.Count);
            tempList.AddRange(eventsRequest.Result.Select(x => new IssueEventItemViewModel(x)));
            tempList.AddRange(commentsRequest.Result.Data.Select(x => new IssueCommentItemViewModel(x)));
            return tempList.OrderBy(x => x.CreatedAt);
        }

        private async Task<Octokit.Issue> UpdateIssue(Octokit.User assignee, Octokit.Milestone milestone, IEnumerable<Octokit.Label> labels)
        {
            var update = new Octokit.IssueUpdate { Assignee = assignee?.Login, Milestone = milestone?.Number };
            update.ClearLabels();
            foreach (var l in (labels ?? Enumerable.Empty<Octokit.Label>())) update.AddLabel(l.Name);

            using (_alertDialogFactory.Activate("Updating..."))
            {
                Issue = await _applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, Id, update);
                InternalEvents.Reset(await RetrieveEvents());
                return Issue;
            }
        }
    }
}

