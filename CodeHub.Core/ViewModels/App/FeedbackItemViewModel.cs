using System;
using ReactiveUI;
using Humanizer;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; }

        public string ImageUrl { get; }

        public ReactiveCommand<Unit, Unit> GoToCommand { get; }
            = ReactiveCommand.Create(() => { });

        public DateTimeOffset Created { get; }

        public string CreatedString { get; }

        internal FeedbackItemViewModel(Octokit.Issue issue)
        {
            Title = issue.Title;
            ImageUrl = issue.User.AvatarUrl;
            Created = issue.CreatedAt;
            CreatedString = Created.Humanize();
        }
    }
}

