using ReactiveUI;
using System;
using CodeHub.Core.Utilities;
using System.Diagnostics;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Repositories
{
    [DebuggerDisplay("{Owner}/{Name}")]
    public class RepositoryItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public string Owner { get; }

        public GitHubAvatar Avatar { get; }

        public string Description { get; }

        public string Stars { get; }

        public string Forks { get; }

        public bool ShowOwner { get; }

        public ReactiveCommand<Unit, Unit> GoToCommand { get; }

        public RepositoryItemViewModel(Octokit.Repository repository, bool showOwner, bool showDescription, Action<RepositoryItemViewModel> gotoCommand)
        {
            if (showDescription)
            {
                if (!string.IsNullOrEmpty(repository.Description) && repository.Description.IndexOf(':') >= 0)
                    Description = Emojis.FindAndReplace(repository.Description);
                else
                    Description = repository.Description;
            }
            else
            {
                Description = null;
            }

            Name = repository.Name;
            Owner = repository.Owner?.Login ?? string.Empty;
            Avatar = new GitHubAvatar(repository.Owner?.AvatarUrl);
            Stars = repository.StargazersCount.ToString();
            Forks = repository.ForksCount.ToString();
            ShowOwner = showOwner;
            GoToCommand = ReactiveCommand.Create(() => gotoCommand?.Invoke(this));
        }
    }
}

