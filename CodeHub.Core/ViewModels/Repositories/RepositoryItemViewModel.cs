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
        public string Name => Repository.Name;

        public string Owner => Repository.Owner?.Login ?? string.Empty;

        public GitHubAvatar Avatar => new GitHubAvatar(Repository.Owner?.AvatarUrl);

        public string Description { get; }

        public string Stars => Repository.StargazersCount.ToString();

        public string Forks => Repository.ForksCount.ToString();
            
        public bool ShowOwner { get; }

        public Octokit.Repository Repository { get; }

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

            Repository = repository;
            ShowOwner = showOwner;
            GoToCommand = ReactiveCommand.Create(() => gotoCommand?.Invoke(this));
        }
    }
}

