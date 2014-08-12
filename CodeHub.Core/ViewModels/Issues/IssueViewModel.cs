using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using System;
using ReactiveUI;

using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private IssueModel _issueModel;

        public long IssueId { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

		public string MarkdownDescription
		{
			get
			{
			    return Issue == null ? string.Empty : (GetService<IMarkdownService>().Convert(Issue.Body));
			}
		}

        public IssueModel Issue
        {
            get { return _issueModel; }
            set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToAssigneeCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestoneCommand { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

		public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand ToggleStateCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<object> AddCommentCommand { get; private set; }

        public ReactiveList<IssueCommentModel> Comments { get; private set; }

        public ReactiveList<IssueEventModel> Events { get; private set; }

        public IssueViewModel(IApplicationService applicationService, IShareService shareService)
        {
            _applicationService = applicationService;
            Comments = new ReactiveList<IssueCommentModel>();
            Events = new ReactiveList<IssueEventModel>();
            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Issue.HtmlUrl));

            AddCommentCommand = ReactiveCommand.Create();
            AddCommentCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<CommentViewModel>();
                ReactiveUI.Legacy.ReactiveCommandMixins.RegisterAsyncTask(vm.SaveCommand, async t =>
                {
                    var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId];
                    var comment = await _applicationService.Client.ExecuteAsync(issue.CreateComment(vm.Comment));
                    Comments.Add(comment.Data);
                    vm.DismissCommand.ExecuteIfCan();
                });
                ShowViewModel(vm);
            });

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(issuePresenceObservable, async t =>
            {
                var close = string.Equals(Issue.State, "open", StringComparison.OrdinalIgnoreCase);
                try
                {
                    var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Issue.Number];
                    var data = await _applicationService.Client.ExecuteAsync(issue.UpdateState(close ? "closed" : "open"));
                    Issue = data.Data;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to " + (close ? "close" : "open") + " the item. " + e.Message, e);
                }
            });

            GoToEditCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToEditCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = IssueId;
                vm.Issue = Issue;
                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                ShowViewModel(vm);
            });

            GoToAssigneeCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToAssigneeCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueAssignedToViewModel>();
                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = IssueId;
                vm.SelectedUser = Issue.Assignee;
                vm.WhenAnyValue(x => x.SelectedUser).Subscribe(x =>
                {
                    Issue.Assignee = x;
                    this.RaisePropertyChanged("Issue");
                });
                ShowViewModel(vm);
            });

            GoToLabelsCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToLabelsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueLabelsViewModel>();
                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = IssueId;
                vm.SelectedLabels.Reset(Issue.Labels);
                vm.WhenAnyValue(x => x.SelectedLabels).Subscribe(x =>
                {
                    Issue.Labels = x.ToList();
                    this.RaisePropertyChanged("Issue");
                });
                ShowViewModel(vm);
            });

            GoToMilestoneCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToMilestoneCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueMilestonesViewModel>();
                vm.SaveOnSelect = true;
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.IssueId = IssueId;
                vm.SelectedMilestone = Issue.Milestone;
                vm.WhenAnyValue(x => x.SelectedMilestone).Subscribe(x =>
                {
                    Issue.Milestone = x;
                    this.RaisePropertyChanged("Issue");
                });
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId];
                var t1 = this.RequestModel(issue.Get(), forceCacheInvalidation, response => Issue = response.Data);
                Comments.SimpleCollectionLoad(issue.GetComments(), forceCacheInvalidation).FireAndForget();
                Events.SimpleCollectionLoad(issue.GetEvents(), forceCacheInvalidation).FireAndForget();
                return t1;
            });
        }
    }
}

