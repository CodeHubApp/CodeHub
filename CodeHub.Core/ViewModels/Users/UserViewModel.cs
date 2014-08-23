using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string Url { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public UserViewModel(string name, string url, Action<UserViewModel> gotoAction)
        {
            Name = name;
            Url = url;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
        }
    }
}

