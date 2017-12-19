using ReactiveUI;
using System;
using CodeHub.Core.Utilities;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Users
{
	public class UserItemViewModel : ReactiveObject, ICanGoToViewModel
	{
        public string Login => User.Login;

        public string Name => User.Name;

        public GitHubAvatar Avatar => new GitHubAvatar(User.AvatarUrl);

        public ReactiveCommand<Unit, Unit> GoToCommand { get; }

        public Octokit.User User { get; }

		internal UserItemViewModel(Octokit.User user, Action<UserItemViewModel> gotoAction)
		{
            User = user;
			GoToCommand = ReactiveCommand.Create(() => gotoAction?.Invoke(this));
		}
	}
}

