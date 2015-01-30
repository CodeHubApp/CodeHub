using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; private set; }

        public DateTime Created { get; private set; }

        public long Id { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal ReleaseItemViewModel(Octokit.Release release)
        {
            Id = release.Id;
            Created = (release.PublishedAt.HasValue ? release.PublishedAt.Value : release.CreatedAt).LocalDateTime;
            Name = string.IsNullOrEmpty(release.Name) ? release.TagName : release.Name;
            GoToCommand = ReactiveCommand.Create();
        }
    }
}

