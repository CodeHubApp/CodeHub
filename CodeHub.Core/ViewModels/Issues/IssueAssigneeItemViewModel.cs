using ReactiveUI;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssigneeItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        internal IssueAssigneeItemViewModel(Octokit.User user)
        {
            Name = user.Login;
            Avatar = new GitHubAvatar(user.AvatarUrl);
            GoToCommand = ReactiveCommand.Create()
                .WithSubscription(_ => IsSelected = !IsSelected);
        }
    }
}

