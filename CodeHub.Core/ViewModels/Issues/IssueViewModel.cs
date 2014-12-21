using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using System;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IMarkdownService _markdownService;

        public int Id { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

		public string MarkdownDescription
		{
			get
			{
                return Issue == null ? string.Empty : (_markdownService.Convert(Issue.Body));
			}
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

        public ReactiveList<IssueCommentModel> Comments { get; private set; }

        public ReactiveList<IssueEventModel> Events { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public IssueViewModel(IApplicationService applicationService, 
            IShareService shareService, IMarkdownService markdownService)
        {
            _applicationService = applicationService;
            _markdownService = markdownService;

            Comments = new ReactiveList<IssueCommentModel>();
            Events = new ReactiveList<IssueEventModel>();
            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Issue.HtmlUrl));

            AddCommentCommand = ReactiveCommand.Create();
            AddCommentCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<CommentViewModel>();
                ReactiveUI.Legacy.ReactiveCommandMixins.RegisterAsyncTask(vm.SaveCommand, async t =>
                {
                    var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id];
                    var comment = await _applicationService.Client.ExecuteAsync(issue.CreateComment(vm.Comment));
                    Comments.Add(comment.Data);
                    Dismiss();
                });
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
                var forceCacheInvalidation = t as bool?;
                var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id];
                Comments.SimpleCollectionLoad(issue.GetComments(), forceCacheInvalidation);
                Events.SimpleCollectionLoad(issue.GetEvents(), forceCacheInvalidation);
                Issue = await applicationService.GitHubClient.Issue.Get(RepositoryOwner, RepositoryName, Id);
            });
        }
    }
}

