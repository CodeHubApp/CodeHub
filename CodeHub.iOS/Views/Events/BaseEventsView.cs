using System;
using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Events;
using GitHubSharp.Models;
using MonoTouch;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Events
{
    public abstract class BaseEventsView : ViewModelCollectionDrivenDialogViewController
    {
        protected BaseEventsView()
        {
            Title = "Events".t();
            Root.UnevenRows = true;
            EnableSearch = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			TableView.SeparatorInset = CodeFramework.iOS.NewsCellView.EdgeInsets;
            BindCollection(((BaseEventsViewModel)ViewModel).Events, CreateElement);
        }

        private static MonoTouch.Dialog.Element CreateElement(Tuple<EventModel, BaseEventsViewModel.EventBlock> e)
        {
            try
            {
                if (e.Item2 == null)
                    return null;

                var img = ChooseImage(e.Item1);
				var username = e.Item1.Actor != null ? e.Item1.Actor.Login : null;
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

				return new NewsFeedElement(username, avatar, e.Item1.CreatedAt, headerBlocks, bodyBlocks, img, e.Item2.Tapped);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Unable to add event", ex);
                return null;
            }
        }

        private static UIImage ChooseImage(EventModel eventModel)
        {
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
                return Images.Comments;

            var createEvent = eventModel.PayloadObject as EventModel.CreateEvent;
            if (createEvent != null)
            {
                var createModel = createEvent;
                if (createModel.RefType.Equals("repository"))
                    return Images.Repo;
                if (createModel.RefType.Equals("branch"))
                    return Images.Branch;
                if (createModel.RefType.Equals("tag"))
                    return Images.Tag;
            }
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
                return Images.BinClosed;
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
                return Images.Following;
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
                return Images.Fork;
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
                return Images.Fork;
            else if (eventModel.PayloadObject is EventModel.GistEvent)
                return Images.Script;
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
                return Images.Webpage;
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
                return Images.Comments;
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
                return Images.Flag;
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
                return Images.Group;
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
                return Images.Heart;
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
                return Images.Hand;
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
                return Images.Comments;
            else if (eventModel.PayloadObject is EventModel.PushEvent)
                return Images.Commit;
            else if (eventModel.PayloadObject is EventModel.TeamAddEvent)
                return Images.Team;
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
                return Images.Star;
			else if (eventModel.PayloadObject is EventModel.ReleaseEvent)
				return Images.Public;
            return Images.Priority;
        }
    }
}