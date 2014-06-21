using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using CodeFramework.Core.Services;
using System;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
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

        public IReactiveCommand GoToAssigneeCommand { get; private set; }

        public IReactiveCommand GoToMilestoneCommand { get; private set; }

        public IReactiveCommand GoToLabelsCommand { get; private set; }

		public IReactiveCommand GoToEditCommand { get; private set; }

        public IReactiveCommand ToggleStateCommand { get; private set; }

        public IReactiveCommand ShareCommand { get; private set; }

        public IReactiveCommand AddCommentCommand { get; private set; }

        public ReactiveCollection<IssueCommentModel> Comments { get; private set; }

        public ReactiveCollection<IssueEventModel> Events { get; private set; }

        public IssueViewModel(IApplicationService applicationService, IShareService shareService)
        {
            _applicationService = applicationService;
            Comments = new ReactiveCollection<IssueCommentModel>();
            Events = new ReactiveCollection<IssueEventModel>();
            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue, x => x != null);

            ShareCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Issue, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Issue.HtmlUrl));

            AddCommentCommand = new ReactiveCommand();
            AddCommentCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<CommentViewModel>();
                vm.SaveCommand.RegisterAsyncTask(async t =>
                {
                    var issue = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId];
                    var comment = await _applicationService.Client.ExecuteAsync(issue.CreateComment(vm.Comment));
                    Comments.Add(comment.Data);
                    vm.DismissCommand.ExecuteIfCan();
                });
                ShowViewModel(vm);
            });

            ToggleStateCommand = new ReactiveCommand(issuePresenceObservable);
            ToggleStateCommand.RegisterAsyncTask(async t =>
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

            GoToEditCommand = new ReactiveCommand(issuePresenceObservable);
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

            GoToAssigneeCommand = new ReactiveCommand(issuePresenceObservable);
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

            GoToLabelsCommand = new ReactiveCommand(issuePresenceObservable);
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

            GoToMilestoneCommand = new ReactiveCommand(issuePresenceObservable);
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

            LoadCommand.RegisterAsyncTask(t =>
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

