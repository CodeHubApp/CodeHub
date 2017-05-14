using ReactiveUI;
using System;
using CodeHub.Core.Utilities;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Users
{
	public class UserItemViewModel : ReactiveObject, ICanGoToViewModel
	{
        public string Login { get; }

		public string Name { get; }

		public GitHubAvatar Avatar { get; }

        public ReactiveCommand<Unit, Unit> GoToCommand { get; }

		internal UserItemViewModel(Octokit.User user, Action<UserItemViewModel> gotoAction)
		{
            Login = user.Login;
            Name = user.Name;
            Avatar = new GitHubAvatar(user.AvatarUrl);
			GoToCommand = ReactiveCommand.Create(() => gotoAction?.Invoke(this));
		}
	}
}

