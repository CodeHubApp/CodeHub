using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;
using System.Reactive;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IMarkdownService _markdownService;

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

        private Octokit.Issue _issue;
        public Octokit.Issue Issue
        {
            get { return _issue; }
            set { this.RaiseAndSetIfChanged(ref _issue, value); }
        }

        private Octokit.PullRequest _pullRequest;
        public Octokit.PullRequest PullRequest
        { 
            get { return _pullRequest; }
            set { this.RaiseAndSetIfChanged(ref _pullRequest, value); }
        }

        public IReactiveCommand GoToAssigneeCommand { get; private set; }

        public IReactiveCommand GoToMilestoneCommand { get; private set; }

        public IReactiveCommand GoToLabelsCommand { get; private set; }

        public IReactiveCommand GoToEditCommand { get; private set; }

        public IReactiveCommand ShareCommand { get; private set; }

        public IReactiveCommand ToggleStateCommand { get; private set; }

        public IReactiveCommand GoToCommitsCommand { get; private set; }

        public IReactiveCommand GoToFilesCommand { get; private set; }

        public IReactiveCommand GoToAddCommentCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public ReactiveList<Octokit.IssueComment> Comments { get; private set; }

        public ReactiveList<Octokit.IssueEvent> Events { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand MergeCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }
 
        public PullRequestViewModel(IApplicationService applicationService, 
            IMarkdownService markdownService, IActionMenuFactory actionMenuService)
        {
            _applicationService = applicationService;
            _markdownService = markdownService;

            Comments = new ReactiveList<Octokit.IssueComment>();
            Events = new ReactiveList<Octokit.IssueEvent>();

            this.WhenAnyValue(x => x.Id).Subscribe(x => Title = "Pull Request #" + x);

            _canMerge = this.WhenAnyValue(x => x.PullRequest)
                .Select(x => x != null && !x.Merged)
                .ToProperty(this, x => x.CanMerge);

            var canMergeObservable = this.WhenAnyValue(x => x.PullRequest).Select(x =>
                x != null && !x.Merged && x.Mergeable.HasValue && x.Mergeable.Value);

            MergeCommand = ReactiveCommand.CreateAsyncTask(canMergeObservable, async t =>
            {
                var req = new Octokit.MergePullRequest(null);
                var response = await _applicationService.GitHubClient.PullRequest.Merge(RepositoryOwner, RepositoryName, Id, req);
                if (!response.Merged)
                    throw new Exception(string.Format("Unable to merge pull request: {0}", response.Message));
                LoadCommand.ExecuteIfCan();
            });

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.PullRequest).Select(x => x != null), 
                async t =>
            {
                var newState = PullRequest.State == Octokit.ItemState.Open ? Octokit.ItemState.Closed : Octokit.ItemState.Open;

                try
                {
                    var req = new Octokit.PullRequestUpdate { State = newState };
                    PullRequest = await _applicationService.GitHubClient.PullRequest.Update(RepositoryOwner, RepositoryName, Id, req);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to " + (newState == Octokit.ItemState.Closed ? "close" : "open") + " the item. " + e.Message, e);
                }
            });

            GoToUrlCommand = ReactiveCommand.Create();
            GoToUrlCommand.OfType<string>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.PullRequest).Select(x => x != null));
            GoToHtmlUrlCommand.Select(_ => PullRequest.HtmlUrl).Subscribe(x => GoToUrlCommand.ExecuteIfCan(x.AbsolutePath));

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
                vm.Username = RepositoryOwner;
                vm.Repository = RepositoryName;
                vm.PullRequestId = Id;
                NavigateTo(vm);
            });
//
//            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.PullRequest).Select(x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)))
//                .WithSubscription(_ => shareService.ShareUrl(PullRequest.HtmlUrl));

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

            GoToLabelsCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<IssueLabelsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SaveOnSelect = true;
//                vm.SelectedLabels.Reset(Issue.Labels);
//                vm.WhenAnyValue(x => x.Labels).Skip(1).Subscribe(x =>
//                {
//                    Issue.Labels = x.ToList();
//                    this.RaisePropertyChanged("Issue");
//                });
                NavigateTo(vm);
            });

            GoToMilestoneCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<IssueMilestonesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SaveOnSelect = true;
//                vm.SelectedMilestone = Issue.Milestone;
//                vm.WhenAnyValue(x => x.SelectedMilestone).Skip(1).Subscribe(x =>
//                {
//                    Issue.Milestone = x;
//                    this.RaisePropertyChanged("Issue");
//                });
                NavigateTo(vm);
            });

            GoToAssigneeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<IssueAssignedToViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                //vm.SaveOnSelect = true;
//                vm.SelectedUser = Issue.Assignee;
//                vm.WhenAnyValue(x => x.SelectedUser).Skip(1).Subscribe(x =>
//                {
//                    Issue.Assignee = x;
//                    this.RaisePropertyChanged("Issue");
//                });
                NavigateTo(vm);
            });

            GoToAddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<IssueCommentViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
                vm.SaveCommand.Subscribe(Comments.Add);
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

            _markdownDescription = this.WhenAnyValue(x => x.PullRequest).IsNotNull()
                .Select(x => _markdownService.Convert(x.Body))
                .ToProperty(this, x => x.MarkdownDescription);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var pullRequest = _applicationService.GitHubClient.PullRequest.Get(RepositoryOwner, RepositoryName, Id);
                var comments = _applicationService.GitHubClient.PullRequest.Comment.GetAll(RepositoryOwner, RepositoryName, Id);
                var events = _applicationService.GitHubClient.Issue.Events.GetForIssue(RepositoryOwner, RepositoryName, Id);
                var issue = _applicationService.GitHubClient.Issue.Get(RepositoryOwner, RepositoryName, Id);

                await Task.WhenAll(pullRequest, issue, comments, events);

                PullRequest = pullRequest.Result;
                Issue = issue.Result;
            });
        }
    }
}
