using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;

using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : BaseViewModel, ILoadableViewModel, ICanGoToUrl
    {
        private readonly IApplicationService _applicationService;
        private readonly IMarkdownService _markdownService;

        public long Id { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private ObservableAsPropertyHelper<string> _markdownDescription;
        public string MarkdownDescription
        {
            get { return _markdownDescription.Value; }
        }

        private bool _merged;
        public bool Merged
        {
            get { return _merged; }
            set { this.RaiseAndSetIfChanged(ref _merged, value); }
        }

        private IssueModel _issue;
        public IssueModel Issue
        {
            get { return _issue; }
            set { this.RaiseAndSetIfChanged(ref _issue, value); }
        }

        private PullRequestModel _pullRequest;
        public PullRequestModel PullRequest
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

        public IReactiveCommand GoToUrlCommand { get; private set; }

        public ReactiveList<IssueCommentModel> Comments { get; private set; }

        public ReactiveList<IssueEventModel> Events { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand MergeCommand { get; private set; }
 
        public PullRequestViewModel(IApplicationService applicationService, IMarkdownService markdownService, IShareService shareService)
        {
            _applicationService = applicationService;
            _markdownService = markdownService;

            GoToUrlCommand = this.CreateUrlCommand();

            Comments = new ReactiveList<IssueCommentModel>();
            Events = new ReactiveList<IssueEventModel>();

            MergeCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.PullRequest).Select(x =>
                x != null && x.Merged.HasValue && !x.Merged.Value && x.Mergable.HasValue && x.Mergable.Value),
                async t =>
                {
                    try
                    {
                        var response = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[RepositoryOwner]
                                                                .Repositories[RepositoryName].PullRequests[Id].Merge());
                        if (!response.Data.Merged)
                            throw new Exception(response.Data.Message);
                        var pullRequest = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[Id].Get();
                        await this.RequestModel(pullRequest, true, r => PullRequest = r.Data);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to Merge: " + e.Message, e);
                    }
                });

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.PullRequest).Select(x => x != null), async t =>
            {
                var close = string.Equals(PullRequest.State, "open", StringComparison.OrdinalIgnoreCase);

                try
                {
                    var data = await _applicationService.Client.ExecuteAsync(
                        _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[Id].UpdateState(close ? "closed" : "open"));
                    PullRequest = data.Data;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to " + (close ? "close" : "open") + " the item. " + e.Message, e);
                }
            });

            GoToCommitsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<PullRequestCommitsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.PullRequestId = Id;
                ShowViewModel(vm);
            });

            GoToFilesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<PullRequestFilesViewModel>();
                vm.Username = RepositoryOwner;
                vm.Repository = RepositoryName;
                vm.PullRequestId = Id;
                ShowViewModel(vm);
            });

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.PullRequest).Select(x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)))
                .WithSubscription(_ => shareService.ShareUrl(PullRequest.HtmlUrl));

            GoToEditCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
                vm.Issue = Issue;
                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                ShowViewModel(vm);
            });

            GoToLabelsCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = CreateViewModel<IssueLabelsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SaveOnSelect = true;
                vm.SelectedLabels.Reset(Issue.Labels);
//                vm.WhenAnyValue(x => x.Labels).Skip(1).Subscribe(x =>
//                {
//                    Issue.Labels = x.ToList();
//                    this.RaisePropertyChanged("Issue");
//                });
                ShowViewModel(vm);
            });

            GoToMilestoneCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = CreateViewModel<IssueMilestonesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SaveOnSelect = true;
                vm.SelectedMilestone = Issue.Milestone;
                vm.WhenAnyValue(x => x.SelectedMilestone).Skip(1).Subscribe(x =>
                {
                    Issue.Milestone = x;
                    this.RaisePropertyChanged("Issue");
                });
                ShowViewModel(vm);
            });

            GoToAssigneeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null)).WithSubscription(_ =>
            {
                var vm = CreateViewModel<IssueAssignedToViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = Id;
                vm.SaveOnSelect = true;
                vm.SelectedUser = Issue.Assignee;
                vm.WhenAnyValue(x => x.SelectedUser).Skip(1).Subscribe(x =>
                {
                    Issue.Assignee = x;
                    this.RaisePropertyChanged("Issue");
                });
                ShowViewModel(vm);
            });

            GoToAddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<CommentViewModel>();
                ReactiveUI.Legacy.ReactiveCommandMixins.RegisterAsyncTask(vm.SaveCommand, async t =>
                {
                    var req =
                        _applicationService.Client.Users[RepositoryOwner]
                        .Repositories[RepositoryName].Issues[Id].CreateComment(vm.Comment);
                    var comment = await _applicationService.Client.ExecuteAsync(req);
                    Comments.Add(comment.Data);
                    vm.DismissCommand.ExecuteIfCan();
                });
            });

            _markdownDescription = this.WhenAnyValue(x => x.PullRequest).IsNotNull()
                .Select(x => _markdownService.Convert(x.Body))
                .ToProperty(this, x => x.MarkdownDescription);

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var pullRequest = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[Id].Get();
                var t1 = this.RequestModel(pullRequest, forceCacheInvalidation, response => PullRequest = response.Data);
                Events.SimpleCollectionLoad(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].GetEvents(), forceCacheInvalidation).FireAndForget();
                Comments.SimpleCollectionLoad(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].GetComments(), forceCacheInvalidation).FireAndForget();
                this.RequestModel(_applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].Get(), forceCacheInvalidation, response => Issue = response.Data).FireAndForget();
                return t1;
            });
        }
    }
}
