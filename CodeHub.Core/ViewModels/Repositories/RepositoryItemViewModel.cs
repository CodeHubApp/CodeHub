using ReactiveUI;
using System;
using CodeHub.Core.Utilities;
using System.Diagnostics;

namespace CodeHub.Core.ViewModels.Repositories
{
    [DebuggerDisplay("{Owner}/{Name}")]
    public class RepositoryItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public string Owner { get; }

        public GitHubAvatar Avatar { get; }

        public string Description { get; }

        public int Stars { get; }

        public int Forks { get; }

        public bool ShowOwner { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        internal Octokit.Repository Repository { get; }

        internal RepositoryItemViewModel(Octokit.Repository repository, bool showOwner, Action<RepositoryItemViewModel> gotoCommand)
        {
            if (!string.IsNullOrEmpty(repository.Description) && repository.Description.IndexOf(':') >= 0)
                Description = Emojis.FindAndReplace(repository.Description);
            else
                Description = repository.Description;

            Repository = repository;
            Name = repository.Name;
            Owner = repository.Owner?.Login ?? string.Empty;
            Avatar = new GitHubAvatar(repository.Owner?.AvatarUrl);
            Stars = repository.StargazersCount;
            Forks = repository.ForksCount;
            ShowOwner = showOwner;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoCommand(this));
        }
    }
}

