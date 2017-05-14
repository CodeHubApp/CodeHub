using System;
using ReactiveUI;
using CodeHub.Core.Utilities;
using Humanizer;
using System.Reactive;
using Octokit;
using System.Linq;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public GitHubAvatar Avatar { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTimeOffset UpdatedAt { get; }

        public string UpdatedString { get; }

        public ReactiveCommand<Unit, Unit> GoToCommand { get; }

        public string Id { get; }

        public Gist Gist { get; }

        private static string GetGistTitle(Gist gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;
            return title;
        }

        public GistItemViewModel(Gist gist, Action<GistItemViewModel> gotoAction)
        {
            Gist = gist;
            Id = gist.Id;
            Title = GetGistTitle(gist);
            Description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            Avatar = new GitHubAvatar(gist.Owner?.AvatarUrl);
            UpdatedAt = gist.UpdatedAt;
            UpdatedString = UpdatedAt.Humanize();
            GoToCommand = ReactiveCommand.Create(() => gotoAction(this));
        }
    }
}

