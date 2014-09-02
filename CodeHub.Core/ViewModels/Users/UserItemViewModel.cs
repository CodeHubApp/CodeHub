using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string Url { get; private set; }

        public bool IsOrganization { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal UserItemViewModel(string name, string url, bool organization, Action<UserItemViewModel> gotoAction)
        {
            Name = name;
            Url = url;
            IsOrganization = organization;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
        }
    }
}

