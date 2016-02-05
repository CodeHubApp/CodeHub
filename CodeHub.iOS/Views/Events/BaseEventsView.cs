using System;
using CodeHub.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Events;
using GitHubSharp.Models;
using MonoTouch;
using UIKit;
using System.Collections.Generic;

namespace CodeHub.iOS.Views.Events
{
    public abstract class BaseEventsView : ViewModelCollectionDrivenDialogViewController
    {
        private static IDictionary<EventType, Octicon> _eventToImage 
        = new Dictionary<EventType, Octicon>
        {
            {EventType.Unknown, Octicon.Alert},
            {EventType.Branch, Octicon.GitBranch},
            {EventType.Comment, Octicon.Comment},
            {EventType.Commit, Octicon.GitCommit},
            {EventType.Delete, Octicon.Trashcan},
            {EventType.Follow, Octicon.Person},
            {EventType.Fork, Octicon.RepoForked},
            {EventType.Gist, Octicon.Gist},
            {EventType.Issue, Octicon.IssueOpened},
            {EventType.Organization, Octicon.Organization},
            {EventType.Public, Octicon.Globe},
            {EventType.PullRequest, Octicon.GitPullRequest},
            {EventType.Repository, Octicon.Repo},
            {EventType.Star, Octicon.Star},
            {EventType.Tag, Octicon.Tag},
            {EventType.Wiki, Octicon.Pencil},
        };

        protected BaseEventsView()
        {
            Title = "Events".t();
            Root.UnevenRows = true;
            EnableSearch = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			TableView.SeparatorInset = CodeHub.iOS.TableViewCells.NewsCellView.EdgeInsets;
            BindCollection(((BaseEventsViewModel)ViewModel).Events, CreateElement);
        }

        private static MonoTouch.Dialog.Element CreateElement(Tuple<EventModel, BaseEventsViewModel.EventBlock> e)
        {
            try
            {
                if (e.Item2 == null)
                    return null;

                var imgKey = ChooseImage(e.Item1);
                var img = Octicon.Alert;
                if (_eventToImage.ContainsKey(imgKey))
                    img = _eventToImage[imgKey];
                    
                var avatar = e.Item1.Actor != null ? e.Item1.Actor.AvatarUrl : null;
				var headerBlocks = new System.Collections.Generic.List<NewsFeedElement.TextBlock>();
				foreach (var h in e.Item2.Header)
				{
					Action act = null;
					var anchorBlock = h as BaseEventsViewModel.AnchorBlock;
					if (anchorBlock != null)
						act = anchorBlock.Tapped;
					headerBlocks.Add(new NewsFeedElement.TextBlock(h.Text, act));
				}

				var bodyBlocks = new System.Collections.Generic.List<NewsFeedElement.TextBlock>();
				foreach (var h in e.Item2.Body)
				{
					Action act = null;
					var anchorBlock = h as BaseEventsViewModel.AnchorBlock;
					if (anchorBlock != null)
						act = anchorBlock.Tapped;
					var block = new NewsFeedElement.TextBlock(h.Text, act);
					if (act == null) block.Color = UIColor.DarkGray;
					bodyBlocks.Add(block);
				}

                return new NewsFeedElement(avatar, e.Item1.CreatedAt, headerBlocks, bodyBlocks, img.ToImage(), e.Item2.Tapped);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Unable to add event", ex);
                return null;
            }
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