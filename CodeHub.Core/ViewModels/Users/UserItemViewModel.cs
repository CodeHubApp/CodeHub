using ReactiveUI;
using System;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public bool IsOrganization { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal UserItemViewModel(string name, string avatarUrl, bool organization, Action gotoAction)
        {
            Name = name;
            Avatar = new GitHubAvatar(avatarUrl);
            IsOrganization = organization;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction());
        }
    }
}

