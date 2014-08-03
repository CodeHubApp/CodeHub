using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssignedToViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        private BasicUserModel _selectedUser;
        public BasicUserModel SelectedUser
        {
            get { return _selectedUser; }
            set { this.RaiseAndSetIfChanged(ref _selectedUser, value); }
        }

        public ReactiveList<BasicUserModel> Users { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public long IssueId { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public bool SaveOnSelect { get; set; }

        public IReactiveCommand SelectUserCommand { get; private set; }

        public IssueAssignedToViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Users = new ReactiveList<BasicUserModel>();

            SelectUserCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var selectedUser = t as BasicUserModel;
                if (selectedUser != null)
                    SelectedUser = selectedUser;

                if (SaveOnSelect)
                {
                    var assignee = SelectedUser != null ? SelectedUser.Login : null;
                    var updateReq = _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId].UpdateAssignee(assignee);
                    await _applicationService.Client.ExecuteAsync(updateReq);
                }

                DismissCommand.ExecuteIfCan();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                Users.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetAssignees(), t as bool?));
        }
    }
}

