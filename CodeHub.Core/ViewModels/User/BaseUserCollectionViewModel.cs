using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.User
{
    public abstract class BaseUserCollectionViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();

        public CollectionViewModel<BasicUserModel> Users
        {
            get { return _users; }
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = x.Login })); }
        }
    }
}
