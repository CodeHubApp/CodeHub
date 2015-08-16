using System;
using ReactiveUI;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Core.Utilities;
using Humanizer;

namespace CodeHub.Core.ViewModels.Activity
{
    public class EventItemViewModel : ReactiveObject
    {
        public GitHubAvatar Avatar { get; }

        public DateTimeOffset Created { get; }

        public string CreatedString { get;  }

        public IReadOnlyCollection<BaseEventsViewModel.TextBlock> HeaderBlocks { get; }

        public IReadOnlyCollection<BaseEventsViewModel.TextBlock> BodyBlocks { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        public EventType Type { get; private set; }

        internal EventModel Event { get; }

        internal EventItemViewModel(
            EventModel eventModel, 
            IReadOnlyCollection<BaseEventsViewModel.TextBlock> headerBlocks, 
            IReadOnlyCollection<BaseEventsViewModel.TextBlock> bodyBlocks)
        {
            Event = eventModel;
            HeaderBlocks = headerBlocks ?? new BaseEventsViewModel.TextBlock[0];
            BodyBlocks = bodyBlocks ?? new BaseEventsViewModel.TextBlock[0];
            GoToCommand = ReactiveCommand.Create();
            Avatar = eventModel.Actor != null ? new GitHubAvatar(eventModel.Actor.AvatarUrl) : GitHubAvatar.Empty;
            Created = eventModel.CreatedAt;
            Type = ChooseImage(eventModel);
            CreatedString = Created.Humanize();
        }

        private static EventType ChooseImage(EventModel eventModel)
        {
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
                return EventType.Comment;

            var createEvent = eventModel.PayloadObject as EventModel.CreateEvent;
            if (createEvent != null)
            {
                var createModel = createEvent;
                if (createModel.RefType.Equals("repository"))
                    return EventType.Repository;
                if (createModel.RefType.Equals("branch"))
                    return EventType.Branch;
                if (createModel.RefType.Equals("tag"))
                    return EventType.Tag;
            }
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
                return EventType.Delete;
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
                return EventType.Follow;
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
                return EventType.Fork;
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
                return EventType.Fork;
            else if (eventModel.PayloadObject is EventModel.GistEvent)
                return EventType.Gist;
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
                return EventType.Wiki;
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
                return EventType.Comment;
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
                return EventType.Issue;
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
                return EventType.Organization;
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
                return EventType.Public;
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
                return EventType.PullRequest;
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
                return EventType.Comment;
            else if (eventModel.PayloadObject is EventModel.PushEvent)
                return EventType.Commit;
            else if (eventModel.PayloadObject is EventModel.TeamAddEvent)
                return EventType.Organization;
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
                return EventType.Star;
            else if (eventModel.PayloadObject is EventModel.ReleaseEvent)
                return EventType.Tag;
            return EventType.Unknown;
        }

        public enum EventType
        {
            Unknown = 0,
            Comment,
            Repository,
            Branch,
            Tag,
            Delete,
            Follow,
            Fork,
            Gist,
            Wiki,
            Issue,
            Organization,
            Public,
            PullRequest,
            Star,
            Commit
        }
    }
}

