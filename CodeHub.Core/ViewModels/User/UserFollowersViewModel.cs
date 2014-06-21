using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.User
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

