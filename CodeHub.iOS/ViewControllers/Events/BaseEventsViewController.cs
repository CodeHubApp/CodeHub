using System;
using CodeHub.iOS.DialogElements;
using CodeHub.Core.ViewModels.Events;
using GitHubSharp.Models;
using UIKit;
using System.Collections.Generic;
using CodeHub.Core.Utilities;
using CodeHub.iOS.ViewControllers.Source;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Events
{
    public abstract class BaseEventsViewController : ViewModelCollectionDrivenDialogViewController
    {
        public new BaseEventsViewModel ViewModel
        {
            get { return (BaseEventsViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

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

        protected BaseEventsViewController()
        {
            Title = "Events";
            EnableSearch = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 64f;
            BindCollection(ViewModel.Events, CreateElement);

            this.OnActivation(d =>
            {
                d(ViewModel.GoToBranchCommand
                  .Subscribe(branch =>
                {
                    var viewController = new SourceTreeViewController(
                        branch.Item1.Owner, branch.Item1.Name, null, branch.Item2, Utilities.ShaType.Branch);
                    this.PushViewController(viewController);
                }));

                d(ViewModel.GoToTagCommand
                  .Subscribe(branch =>
                  {
                      var viewController = new SourceTreeViewController(
                          branch.Item1.Owner, branch.Item1.Name, null, branch.Item2, Utilities.ShaType.Tag);
                      this.PushViewController(viewController);
                  }));
            });
        }

        private static Element CreateElement(Tuple<EventModel, BaseEventsViewModel.EventBlock> e)
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
                var headerBlocks = new List<NewsFeedElement.TextBlock>();
                foreach (var h in e.Item2.Header)
                {
                    Action act = null;
                    var anchorBlock = h as BaseEventsViewModel.AnchorBlock;
                    if (anchorBlock != null)
                        act = anchorBlock.Tapped;
                    headerBlocks.Add(new NewsFeedElement.TextBlock(h.Text, act));
                }

                var bodyBlocks = new List<NewsFeedElement.TextBlock>();
                foreach (var h in e.Item2.Body)
                {
                    Action act = null;
                    var anchorBlock = h as BaseEventsViewModel.AnchorBlock;
                    if (anchorBlock != null)
                        act = anchorBlock.Tapped;
                    var block = new NewsFeedElement.TextBlock(h.Text, act);
                    bodyBlocks.Add(block);
                }

                var weakTapped = new WeakReference<Action>(e.Item2.Tapped);
                var githubAvatar = new GitHubAvatar(avatar).ToUri(64)?.AbsoluteUri;
                return new NewsFeedElement(githubAvatar, e.Item1.CreatedAt, headerBlocks, bodyBlocks, img.ToImage(), () => weakTapped.Get()?.Invoke(), e.Item2.Multilined);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to add event: " + ex.Message);
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