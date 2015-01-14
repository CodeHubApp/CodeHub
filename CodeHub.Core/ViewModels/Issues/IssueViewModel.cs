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

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseViewModel, ILoadableViewModel
    {
        private int _id;
        public int Id 
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private readonly ObservableAsPropertyHelper<string> _markdownDescription;
		public string MarkdownDescription
		{
            get { return _markdownDescription.Value; }
		}

        private Octokit.Issue _issueModel;
        public Octokit.Issue Issue
        {
            get { return _issueModel; }
            set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToAssigneeCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestoneCommand { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

		public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand ToggleStateCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<object> AddCommentCommand { get; private set; }

        public IReadOnlyReactiveList<IIssueEventItemViewModel> Events { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public IssueViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuFactory,
            IShareService shareService, IMarkdownService markdownService)
        {
            this.WhenAnyValue(x => x.Id).Subscribe(x => Title = "Issue #" + x);

            _markdownDescription = this.WhenAnyValue(x => x.Issue).Select(x =>
            {
                return ((x == null || string.IsNullOrEmpty(x.Body)) ? null : markdownService.Convert(x.Body));
            }).ToProperty(this, x => x.MarkdownDescription);

            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue).Select(x => x != null);

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

            GoToAssigneeCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToAssigneeCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueAssignedToViewModel>();
//                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SelectedUser = Issue.Assignee;
                vm.WhenAnyValue(x => x.SelectedUser).Subscribe(x =>
                {
                    Issue.Assignee = x;
                    this.RaisePropertyChanged("Issue");
                });
                NavigateTo(vm);
            });

            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToLabelsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueLabelsViewModel>();
                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SelectedLabels.Reset(Issue.Labels);
                vm.WhenAnyValue(x => x.SelectedLabels).Subscribe(x =>
                {
                    Issue.Labels = x.ToList();
                    this.RaisePropertyChanged("Issue");
                });
                NavigateTo(vm);
            });

            GoToMilestoneCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToMilestoneCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueMilestonesViewModel>();
                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SelectedMilestone = Issue.Milestone;
                vm.WhenAnyValue(x => x.SelectedMilestone).Subscribe(x =>
                {
                    Issue.Milestone = x;
                    this.RaisePropertyChanged("Issue");
                });
                NavigateTo(vm);
            });

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
    }
}

