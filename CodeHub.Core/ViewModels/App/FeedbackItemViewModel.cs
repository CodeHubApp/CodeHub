using System;
using ReactiveUI;
using Humanizer;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; }

        public string ImageUrl { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        public DateTimeOffset Created { get; }

        public string CreatedString { get; }

        internal FeedbackItemViewModel(Octokit.Issue issue, Action gotoAction)
        {
            Title = issue.Title;
            ImageUrl = issue.User.AvatarUrl;
            Created = issue.CreatedAt;
            CreatedString = Created.Humanize();
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction());
        }
    }
}

