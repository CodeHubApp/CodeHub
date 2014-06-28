using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel
    {
        public string Username { get; set; }

        public UserFollowersViewModel(IApplicationService applicationService)
        {
            LoadCommand.RegisterAsyncTask(t =>
                    Users.SimpleCollectionLoad(applicationService.Client.Users[Username].GetFollowers(), t as bool?));
        }
    }
}

