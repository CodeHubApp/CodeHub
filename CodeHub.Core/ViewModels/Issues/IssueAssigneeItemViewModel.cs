using ReactiveUI;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssigneeItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public GitHubAvatar Avatar { get;}

        public IReactiveCommand<object> GoToCommand { get; }

        internal Octokit.User User { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        internal IssueAssigneeItemViewModel(Octokit.User user)
        {
            Name = user.Login;
            User = user;
            Avatar = new GitHubAvatar(user.AvatarUrl);
            GoToCommand = ReactiveCommand.Create()
                .WithSubscription(_ => IsSelected = !IsSelected);
        }
    }
}

