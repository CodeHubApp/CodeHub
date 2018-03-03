using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Windows.Input;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using System;
using MvvmCross.Core.ViewModels;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.User;
using System.Reactive;
using Splat;
using System.Reactive.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
        private IDisposable _editToken;
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;
        private readonly IMessageService _messageService;
        private readonly IMarkdownService _markdownService;

        public long Id 
        { 
            get; 
            private set; 
        }

        public string Username 
        { 
            get; 
            private set; 
        }

        public string Repository 
        { 
            get; 
            private set; 
        }

        private string _markdownDescription;
        public string MarkdownDescription
        {
            get { return _markdownDescription; }
            private set { this.RaiseAndSetIfChanged(ref _markdownDescription, value); }
        }

        private bool? _isClosed;
        public bool? IsClosed
        {
            get { return _isClosed; }
            private set { this.RaiseAndSetIfChanged(ref _isClosed, value); }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        private bool _isCollaborator;
        public bool IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private IssueModel _issueModel;
        public IssueModel Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private bool _isModifying;
        public bool IsModifying
        {
            get { return _isModifying; }
            set { this.RaiseAndSetIfChanged(ref _isModifying, value); }
        }

        private int? _participants;
        public int? Participants
        {
            get { return _participants; }
            set { this.RaiseAndSetIfChanged(ref _participants, value); }
        }

        private IReadOnlyList<Octokit.IssueComment> _comments;
        public IReadOnlyList<Octokit.IssueComment> Comments
        {
            get { return _comments ?? new List<Octokit.IssueComment>(); }
            private set { this.RaiseAndSetIfChanged(ref _comments, value); }
        }

        private IReadOnlyList<Octokit.EventInfo> _events;
        public IReadOnlyList<Octokit.EventInfo> Events
        {
            get { return _events ?? new List<Octokit.EventInfo>(); }
            private set { this.RaiseAndSetIfChanged(ref _events, value); }
        }

        public ReactiveUI.ReactiveCommand<Unit, bool> GoToOwner { get; }

        public ICommand GoToAssigneeCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<IViewModelTxService>().Add(Issue.Assignee);
                    ShowViewModel<IssueAssignedToViewModel>(new IssueAssignedToViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
            }
        }

        public ICommand GoToMilestoneCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<IViewModelTxService>().Add(Issue.Milestone);
                    ShowViewModel<IssueMilestonesViewModel>(new IssueMilestonesViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
            }
        }

        public ICommand GoToLabelsCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<IViewModelTxService>().Add(Issue.Labels);
                    ShowViewModel<IssueLabelsViewModel>(new IssueLabelsViewModel.NavObject { Username = Username, Repository = Repository, Id = Id, SaveOnSelect = true });
                }, () =>  IsCollaborator); 
            }
        }

        public ICommand GoToEditCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<IViewModelTxService>().Add(Issue);
                    ShowViewModel<IssueEditViewModel>(new IssueEditViewModel.NavObject { Username = Username, Repository = Repository, Id = Id });
                }, () => Issue != null && IsCollaborator); 
            }
        }

        public ICommand ToggleStateCommand
        {
            get 
            {
                return new MvxCommand(() => ToggleState(Issue.State == "open"), () => Issue != null);
            }
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                _applicationService
                    .GitHubClient.Repository.Get(Username, Repository)
                    .ToBackground(x => ShouldShowPro = x.Private && !_featuresService.IsProEnabled);
            }

            _applicationService
                .GitHubClient.Issue.Comment.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(x => Comments = x);

            _applicationService
                .GitHubClient.Issue.Events.GetAllForIssue(Username, Repository, (int)Id)
                .ToBackground(events =>
                {
                    Events = events;
                    Participants = events.Select(x => x.Actor.Login).Distinct().Count();
                });

            _applicationService
                .GitHubClient.Repository.Collaborator.IsCollaborator(Username, Repository, _applicationService.Account.Username)
                .ToBackground(x => IsCollaborator = x);

            return this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), response => Issue = response.Data);
        }

        public IssueViewModel(
            IApplicationService applicationService = null,
            IFeaturesService featuresService = null,
            IMessageService messageService = null,
            IMarkdownService markdownService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();
            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();

            this.Bind(x => x.Issue, true)
                .Where(x => x != null)
                .Select(x => string.Equals(x.State, "closed"))
                .Subscribe(x => IsClosed = x);

            this.Bind(x => x.Issue, true)
                .SelectMany(issue => _markdownService.Convert(issue?.Body).ToObservable())
                .Subscribe(x => MarkdownDescription = x);

            GoToOwner = ReactiveUI.ReactiveCommand.Create(
                () => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Issue?.User?.Login }),
                this.Bind(x => x.Issue, true).Select(x => x != null));
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;

            _editToken = _messageService.Listen<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Issue.Number)
                    return;
                Issue = x.Issue;
            });
        }

        public async Task<bool> AddComment(string text)
        {
            try
            {
                var comment = await _applicationService.GitHubClient.Issue.Comment.Create(Username, Repository, (int)Id, text);
                var newCommentList = new List<Octokit.IssueComment>(Comments) { comment };
                Comments = newCommentList;
                return true;
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return false;
            }
        }

        private async Task ToggleState(bool closed)
        {
            try
            {
                IsModifying = true;
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Issue.Number].UpdateState(closed ? "closed" : "open")); 
                _messageService.Send(new IssueEditMessage(data.Data));
            }
            catch (Exception e)
            {
                DisplayAlert("Unable to " + (closed ? "close" : "open") + " the item. " + e.Message);
            }
            finally
            {
                IsModifying = false;
            }
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
        }
    }
}

