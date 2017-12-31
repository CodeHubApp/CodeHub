using System;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssignedToViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;

        private BasicUserModel _selectedUser;
        public BasicUserModel SelectedUser
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

        private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();
        public CollectionViewModel<BasicUserModel> Users
        {
            get { return _users; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IssueAssignedToViewModel(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public void Init(NavObject navObject) 
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;

            SelectedUser = TxSevice.Get() as BasicUserModel;
            this.Bind(x => x.SelectedUser).Subscribe(x => SelectUser(x));
        }

        private async Task SelectUser(BasicUserModel x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;
                    var assignee = x != null ? x.Login : null;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateAssignee(assignee);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    _messageService.Send(new IssueEditMessage(newIssue.Data));
        
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

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetAssignees());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
            public bool SaveOnSelect { get; set; }
        }
    }
}

