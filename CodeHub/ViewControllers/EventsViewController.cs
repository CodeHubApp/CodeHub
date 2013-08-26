using System;
using System.Linq;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeHub.ViewControllers;
using MonoTouch.Foundation;

namespace CodeHub.ViewControllers
{
    public class EventsViewController : BaseListControllerDrivenViewController, IListView<EventModel>
    {
        public string Username { get; private set; }

        public bool ReportRepository { get; set; }

        public EventsViewController(string username)
        {
            Title = "Events".t();
            Username = username;
            Root.UnevenRows = true;
            ReportRepository = true;
            EnableSearch = false;
            Controller = new EventsController(this, username);
        }

        public void Render(ListModel<EventModel> model)
        {
            RenderList(model, e => {
                try
                {
                    UIImage small;
                    Action elementAction;
                    var hello = CreateDescription(e, out small, out elementAction);
                    if (hello == null)
                        return null;

                    //Get the user
                    var username = e.Actor != null ? e.Actor.Login : null;
                    var avatar = e.Actor != null ? e.Actor.AvatarUrl : null;
                    var newsEl = new NewsFeedElement(username, avatar, (e.CreatedAt), hello, small);
                    if (elementAction != null)
                        newsEl.Tapped += () => elementAction();
                    return newsEl;
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Unable to add event", ex);
                    return null;
                }
            });
        }


        private IEnumerable<NewsFeedElement.TextBlock> CreateDescription(EventModel eventModel, out UIImage img, out Action elementAction)
        {
            var blocks = new List<NewsFeedElement.TextBlock>(10);
            var repoId = eventModel.Repo != null ? new CodeHub.Utils.RepositoryIdentifier(eventModel.Repo.Name) : new CodeHub.Utils.RepositoryIdentifier();
            img = Images.Priority;
            elementAction = null;

            //Drop the image
            if (eventModel.PayloadObject is EventModel.PushEvent)
            {
                img = Images.Plus;
                var pushEvent = (EventModel.PushEvent)eventModel.PayloadObject;
                var desc = pushEvent.Commits[0].Message ?? "".Replace('\n', ' ');

                if (eventModel.Repo != null)
                    elementAction = () => NavigationController.PushViewController(new ChangesetInfoViewController(repoId.Owner, repoId.Name, pushEvent.Commits[0].Sha) { Repo = repoId }, true);

                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Commited to ".t()));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(": " + desc));
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Commited: ".t() + desc));
            }
            else if (eventModel.PayloadObject is EventModel.CreateEvent)
            {
                img = Images.Create;
                var createModel = (EventModel.CreateEvent)eventModel.PayloadObject;

                if (createModel.RefType.Equals("repository"))
                {
                    if (ReportRepository)
                    {
                        elementAction = RepoAction(eventModel.Repo);
                        blocks.Add(new NewsFeedElement.TextBlock("Created repository ".t()));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                    else
                        blocks.Add(new NewsFeedElement.TextBlock("Repository created".t()));
                }
                else if (createModel.RefType.Equals("branch"))
                {
                    var act = elementAction = () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created branch ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(createModel.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
                else if (createModel.RefType.Equals("tag"))
                {
                    var act = elementAction = () => NavigationController.PushViewController(new TagsViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created tag ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(createModel.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
            }
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
            {
                img = Images.BinClosed;
                var deleteEvent = (EventModel.DeleteEvent)eventModel.PayloadObject;

                if (deleteEvent.RefType.Equals("branch"))
                {
                    var act = elementAction = () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted branch ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(deleteEvent.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
                else if (deleteEvent.RefType.Equals("tag"))
                {
                    var act = elementAction = () => NavigationController.PushViewController(new TagsViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted tag ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(deleteEvent.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
            }
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
            {
                img = Images.BinClosed;
                var followEvent = (EventModel.FollowEvent)eventModel.PayloadObject;
            }
//            else if (eventModel.Event == EventModel.Type.WikiUpdated)
//            {
//                img = Images.Pencil;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new WikiViewController(eventModel.Repository.Owner, eventModel.Repository.Slug, eventModel.Description), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Updated wiki page "));
//                blocks.Add(new NewsFeedElement.TextBlock(desc.TrimStart('/'), UIFont.BoldSystemFontOfSize(12f), UIColor.FromRGB(0, 64, 128)));
//
//                if (ReportRepository)
//                {
//                    blocks.Add(new NewsFeedElement.TextBlock(" in "));
//                    blocks.AddRange(RepoName(eventModel));
//                }
//            }
//            else if (eventModel.Event == EventModel.Type.WikiCreated)
//            {
//                img = Images.Pencil;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new WikiViewController(eventModel.Repository.Owner, eventModel.Repository.Slug, eventModel.Description), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Created wiki page "));
//                blocks.Add(new NewsFeedElement.TextBlock(desc.TrimStart('/'), UIFont.BoldSystemFontOfSize(12f), UIColor.FromRGB(0, 64, 128)));
//
//                if (ReportRepository)
//                {
//                    blocks.Add(new NewsFeedElement.TextBlock(" in "));
//                    blocks.AddRange(RepoName(eventModel));
//                }
//            }
//            else if (eventModel.Event == EventModel.Type.WikiDeleted)
//            {
//                img = Images.BinClosed;
//                blocks.Add(new NewsFeedElement.TextBlock("Deleted wiki page "));
//                blocks.Add(new NewsFeedElement.TextBlock(desc.TrimStart('/'), UIFont.BoldSystemFontOfSize(12f), UIColor.FromRGB(0, 64, 128)));
//                
//                if (ReportRepository)
//                {
//                    blocks.Add(new NewsFeedElement.TextBlock(" in "));
//                    blocks.AddRange(RepoName(eventModel));
//                }
//            }
//            else if (eventModel.Event == EventModel.Type.StartFollowUser)
//            {
//                img = Images.HeartAdd;
//                blocks.Add(new NewsFeedElement.TextBlock("Started following a user"));
//            }
//            else if (eventModel.Event == EventModel.Type.StartFollowRepo)
//            {
//                img = Images.HeartAdd;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(eventModel.Repository), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Started following "));
//                blocks.AddRange(RepoName(eventModel));
//            }
//            else if (eventModel.Event == EventModel.Type.StopFollowRepo)
//            {
//                img = Images.HeartDelete;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(eventModel.Repository), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Stopped following "));
//                blocks.AddRange(RepoName(eventModel));
//            }
//            else if (eventModel.Event == EventModel.Type.IssueComment)
//            {
//                img = Images.CommentAdd;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new IssuesViewController(eventModel.Repository.Owner, eventModel.Repository.Slug), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Issue commented on in "));
//                blocks.AddRange(RepoName(eventModel));
//            }
//            else if (eventModel.Event == EventModel.Type.IssueUpdated)
//            {
//                img = Images.ReportEdit;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new IssuesViewController(eventModel.Repository.Owner, eventModel.Repository.Slug), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Issue updated in "));
//                blocks.AddRange(RepoName(eventModel));
//            }
//            else if (eventModel.Event == EventModel.Type.IssueReported)
//            {
//                img = Images.ReportEdit;
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new IssuesViewController(eventModel.Repository.Owner, eventModel.Repository.Slug), true);
//                blocks.Add(new NewsFeedElement.TextBlock("Issue reported on in "));
//                blocks.AddRange(RepoName(eventModel));
//            }
//            else if (eventModel.Event == EventModel.Type.ChangeSetCommentCreated || eventModel.Event == EventModel.Type.ChangeSetCommentDeleted || eventModel.Event == EventModel.Type.ChangeSetCommentUpdated
//                     || eventModel.Event == EventModel.Type.ChangeSetLike || eventModel.Event == EventModel.Type.ChangeSetUnlike)
//            {
//                if (eventModel.Repository != null)
//                    elementAction = () => NavigationController.PushViewController(new ChangesetInfoViewController(eventModel.Repository.Owner, eventModel.Repository.Slug, eventModel.Node), true);
//
//                if (eventModel.Event == EventModel.Type.ChangeSetCommentCreated)
//                {
//                    img = Images.CommentAdd;
//                    blocks.Add(new NewsFeedElement.TextBlock("Commented on commit "));
//                }
//                else if (eventModel.Event == EventModel.Type.ChangeSetCommentDeleted)
//                {
//                    img = Images.BinClosed;
//                    blocks.Add(new NewsFeedElement.TextBlock("Deleted a comment on commit "));
//                }
//                else if (eventModel.Event == EventModel.Type.ChangeSetCommentUpdated)
//                {
//                    img = Images.Pencil;
//                    blocks.Add(new NewsFeedElement.TextBlock("Updated a comment on commit "));
//                }
//                else if (eventModel.Event == EventModel.Type.ChangeSetLike)
//                {
//                    img = Images.Accept;
//                    blocks.Add(new NewsFeedElement.TextBlock("Approved commit "));
//                }
//                else if (eventModel.Event == EventModel.Type.ChangeSetUnlike)
//                {
//                    img = Images.Cancel;
//                    blocks.Add(new NewsFeedElement.TextBlock("Unapproved commit "));
//                }
//
//                var nodeBlock = CommitBlock(eventModel);
//                if (nodeBlock != null)
//                    blocks.Add(nodeBlock);
//                blocks.Add(new NewsFeedElement.TextBlock(" in "));
//                blocks.AddRange(RepoName(eventModel));
//
//            }
//            else
//                return null;

            return blocks;
        }

//        private NewsFeedElement.TextBlock CommitBlock(EventModel e)
//        {
//            var node = e.Node;
//            if (string.IsNullOrEmpty(node))
//                return null;
//            node = node.Substring(0, node.Length > 10 ? 10 : node.Length);
//            return new NewsFeedElement.TextBlock(node, () => {
//                NavigationController.PushViewController(new ChangesetInfoViewController(e.Repo.Owner, e.Repository.Slug, e.Node), true);
//            });
//        }
//        
        private void RepoTapped(EventModel.RepoModel e)
        {
            if (e != null && ValidRepo(e))
            {
                var repoNameSplit = e.Name.Split('/');
                NavigationController.PushViewController(new RepositoryInfoViewController(repoNameSplit[0], repoNameSplit[1], repoNameSplit[1]), true);
            }
        }

        private Action RepoAction(EventModel.RepoModel e)
        {            
            if (e != null && ValidRepo(e))
            {
                var repoNameSplit = e.Name.Split('/');
                return () => NavigationController.PushViewController(new RepositoryInfoViewController(repoNameSplit[0], repoNameSplit[1]), true);
            }
            return null;
        }

        private NewsFeedElement.TextBlock CreateUserBlock(string username)
        {
            return new NewsFeedElement.TextBlock(username, () => NavigationController.PushViewController(new ProfileViewController(username), true));
        }

        private NewsFeedElement.TextBlock CreateIssueBlock(IssueModel issue)
        {
            return new NewsFeedElement.TextBlock(issue.Title, () => { });
        }

        private NewsFeedElement.TextBlock CreateWikiBlock(EventModel.GollumEvent.PageModel page)
        {
            return new NewsFeedElement.TextBlock(page.Title,() => { });
        } 

        private NSAction NavigateToRepository(EventModel.RepoModel repoModel)
        {
            if (ValidRepo(repoModel))
            {
                var repoNameSplit = repoModel.Name.Split('/');
                return () => NavigationController.PushViewController(new RepositoryInfoViewController(repoNameSplit[0], repoNameSplit[1], repoNameSplit[1]), true);
            }

            return null;
        }

        private bool ValidRepo(EventModel.RepoModel repoModel)
        {
            return (repoModel != null && repoModel.Name != null);
        }

        private IEnumerable<NewsFeedElement.TextBlock> RepoName(EventModel.RepoModel repoModel)
        {
            //Most likely indicates a deleted repository
            if (repoModel == null)
                return new [] { new NewsFeedElement.TextBlock("Unknown Repository") };
            if (repoModel.Name == null)
                return new [] { new NewsFeedElement.TextBlock("<Deleted Repository>", color: UIColor.Red) };

            var repoSplit = repoModel.Name.Split('/');
            if (repoSplit.Length < 2)
            {
                return new [] { new NewsFeedElement.TextBlock(repoModel.Name) };
            }

            var repoOwner = repoSplit[0];
            var repoName = repoSplit[1];
            if (!repoOwner.ToLower().Equals(Application.Accounts.ActiveAccount.Username.ToLower()))
            {
                return new [] {
                    new NewsFeedElement.TextBlock(repoOwner, () => NavigationController.PushViewController(new ProfileViewController(repoOwner), true)),
                    new NewsFeedElement.TextBlock("/", UIFont.BoldSystemFontOfSize(12f)),
                    new NewsFeedElement.TextBlock(repoName, () => RepoTapped(repoModel)),
                };
            }

            //Just return the name
            return new [] { new NewsFeedElement.TextBlock(repoName, () => RepoTapped(repoModel)) };
        }
    }
}