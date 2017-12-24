using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssignedToViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;

        private Octokit.User _selectedUser;
        public Octokit.User SelectedUser
        {
            get
            {
                return _selectedUser;
            }
            set
            {
                _selectedUser = value;
                RaisePropertyChanged(() => SelectedUser);
            }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set {
                _isSaving = value;
                RaisePropertyChanged(() => IsSaving);
            }
        }

        private readonly CollectionViewModel<Octokit.User> _users = new CollectionViewModel<Octokit.User>();
        public CollectionViewModel<Octokit.User> Users
        {
            get { return _users; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public int Id { get; private set; }

        public bool SaveOnSelect { get; private set; }


        public IssueAssignedToViewModel(IMessageService messageService, IApplicationService applicationService)
        {
            _messageService = messageService;
            _applicationService = applicationService;
        }

        public void Init(NavObject navObject) 
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;

            SelectedUser = TxSevice.Get() as Octokit.User;

            this.Bind(x => x.SelectedUser)
                .SelectMany(x => SelectUser(x).ToObservable())
                .Subscribe();
        }
       
        private async Task SelectUser(Octokit.User x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;

                    var issueUpdate = new Octokit.IssueUpdate();

                    if (x == null)
                        issueUpdate.ClearAssignees();
                    else
                        issueUpdate.AddAssignee(x.Login);

                    var newIssue = await _applicationService.GitHubClient.Issue.Update(Username, Repository, Id, issueUpdate);
                    _messageService.Send(new IssueEditMessage(newIssue));
        
                }
                catch
                {
                    DisplayAlert("Unable to assign issue to selected user! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                _messageService.Send(new SelectedAssignedToMessage(x));
            }

            ChangePresentation(new MvxClosePresentationHint(this));
        }

        protected override async Task Load()
        {
            var users = await _applicationService.GitHubClient.Issue.Assignee.GetAllForRepository(Username, Repository);
            Users.Items.Reset(users);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public int Id { get; set; }
            public bool SaveOnSelect { get; set; }
        }
    }
}

