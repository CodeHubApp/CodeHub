using System;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using MonoTouch.Foundation;

namespace CodeHub.ViewControllers
{
    public abstract class BaseEventsViewController : BaseListControllerDrivenViewController, IListView<EventModel>
    {
        public bool ReportRepository { get; set; }

        protected BaseEventsViewController()
        {
            Title = "Events".t();
            Root.UnevenRows = true;
            ReportRepository = true;
            EnableSearch = false;
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

            /*
             * COMMIT COMMENT EVENT
             */
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
            {
                var commitEvent = (EventModel.CommitCommentEvent)eventModel.PayloadObject;
                img = Images.CommentAdd;
                var action = elementAction = () => NavigationController.PushViewController(new ChangesetInfoViewController(repoId.Owner, repoId.Name, commitEvent.Comment.CommitId), true);
                var node = commitEvent.Comment.CommitId.Substring(0, commitEvent.Comment.CommitId.Length > 10 ? 10 : commitEvent.Comment.CommitId.Length);
                blocks.Add(new NewsFeedElement.TextBlock("Commented on commit ".t()));
                blocks.Add(new NewsFeedElement.TextBlock(node, () => action()));

                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                    blocks.Add(RepoName(eventModel.Repo));
                }

                blocks.Add(new NewsFeedElement.TextBlock(": "));
                blocks.Add(new NewsFeedElement.TextBlock(commitEvent.Comment.Body));
            }
            /*
             * CREATE EVENT
             */
            else if (eventModel.PayloadObject is EventModel.CreateEvent)
            {
                var createModel = (EventModel.CreateEvent)eventModel.PayloadObject;

                if (createModel.RefType.Equals("repository"))
                {
                    img = Images.Repo;
                    if (ReportRepository)
                    {
                        elementAction = RepoAction(eventModel.Repo);
                        blocks.Add(new NewsFeedElement.TextBlock("Created repository ".t()));
                        blocks.Add(RepoName(eventModel.Repo));
                    }
                    else
                        blocks.Add(new NewsFeedElement.TextBlock("Repository created".t()));
                }
                else if (createModel.RefType.Equals("branch"))
                {
                    img = Images.Branch;
                    var act = elementAction = () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created branch ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(createModel.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.Add(RepoName(eventModel.Repo));
                    }
                }
                else if (createModel.RefType.Equals("tag"))
                {
                    img = Images.Tag;
                    var act = elementAction = () => NavigationController.PushViewController(new TagsViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created tag ".t()));
                    blocks.Add(new NewsFeedElement.TextBlock(createModel.Ref, () => act()));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                        blocks.Add(RepoName(eventModel.Repo));
                    }
                }
            }
            /*
             * DELETE EVENT
             */
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
            {
                img = Images.BinClosed;
                var deleteEvent = (EventModel.DeleteEvent)eventModel.PayloadObject;
                Action act = null;

                if (deleteEvent.RefType.Equals("branch"))
                {
                    act = elementAction = () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted branch ".t()));
                }
                else if (deleteEvent.RefType.Equals("tag"))
                {
                    act = elementAction = () => NavigationController.PushViewController(new TagsViewController(repoId.Owner, repoId.Name), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted tag ".t()));
                }
                else
                    return null;

                blocks.Add(new NewsFeedElement.TextBlock(deleteEvent.Ref, () => act()));
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                    blocks.Add(RepoName(eventModel.Repo));
                }
            }
            /*
             * DOWNLOAD EVENT
             */
            else if (eventModel.PayloadObject is EventModel.DownloadEvent)
            {
                // Don't show the download event for now...
                return null;
            }
            /*
             * FOLLOW EVENT
             */
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
            {
                img = Images.User;
                var followEvent = (EventModel.FollowEvent)eventModel.PayloadObject;
                var action = elementAction = () => NavigationController.PushViewController(new ProfileViewController(followEvent.Target.Login), true);
                blocks.Add(new NewsFeedElement.TextBlock("Started following ".t()));
                blocks.Add(new NewsFeedElement.TextBlock(followEvent.Target.Login, () => action()));
            }
            /*
             * FORK EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
            {
                img = Images.Fork;
                var forkEvent = (EventModel.ForkEvent)eventModel.PayloadObject;
                elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(forkEvent.Forkee.Owner.Login, forkEvent.Forkee.Name), true);
                blocks.Add(new NewsFeedElement.TextBlock("Forked ".t()));
                blocks.Add(RepoName(eventModel.Repo));
                blocks.Add(new NewsFeedElement.TextBlock(" to ".t()));
                blocks.Add(RepoName(new EventModel.RepoModel { Id = forkEvent.Forkee.Id, Name = forkEvent.Forkee.FullName, Url = forkEvent.Forkee.Url }));
            }
            /*
             * FORK APPLY EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
            {
                img = Images.Fork;
                var forkEvent = (EventModel.ForkApplyEvent)eventModel.PayloadObject;
                elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(repoId.Owner, repoId.Name), true);
                blocks.Add(new NewsFeedElement.TextBlock("Applied fork to ".t()));
                blocks.Add(RepoName(eventModel.Repo));
                blocks.Add(new NewsFeedElement.TextBlock(" on branch ".t()));
                blocks.Add(new NewsFeedElement.TextBlock(forkEvent.Head, () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true)));
            }
            /*
             * GIST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.GistEvent)
            {
                img = Images.Language;
                var gistEvent = (EventModel.GistEvent)eventModel.PayloadObject;
                var action = elementAction = () => NavigationController.PushViewController(new GistInfoViewController(gistEvent.Gist.Id), true);

                if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
                    blocks.Add(new NewsFeedElement.TextBlock("Created Gist #".t()));
                else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
                    blocks.Add(new NewsFeedElement.TextBlock("Updated Gist #".t()));

                blocks.Add(new NewsFeedElement.TextBlock(gistEvent.Gist.Id, () => action()));
                blocks.Add(new NewsFeedElement.TextBlock(": " + gistEvent.Gist.Description.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * GOLLUM EVENT (WIKI)
             */
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
            {
                img = Images.Webpage;
                var gistEvent = (EventModel.GollumEvent)eventModel.PayloadObject;
//                var action = elementAction = () => NavigationController.PushViewController(new GistInfoViewController(gistEvent.Gist.Id), true);
//                if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
//                {
//                    blocks.Add(new NewsFeedElement.TextBlock("Created Gist #".t()));
//                    blocks.Add(new NewsFeedElement.TextBlock(gistEvent.Gist.Id, () => action()));
//                    blocks.Add(new NewsFeedElement.TextBlock(": " + gistEvent.Gist.Description));
//                }
//                else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
//                {
//                    blocks.Add(new NewsFeedElement.TextBlock("Updated Gist #".t()));
//                    blocks.Add(new NewsFeedElement.TextBlock(gistEvent.Gist.Id, () => action()));
//                    blocks.Add(new NewsFeedElement.TextBlock(": " + gistEvent.Gist.Description));
//                }
            }
            /*
             * ISSUE COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
            {
                img = Images.CommentAdd;
                var commentEvent = (EventModel.IssueCommentEvent)eventModel.PayloadObject;
                var action = elementAction = () => NavigationController.PushViewController(new IssueViewController(repoId.Owner, repoId.Name, commentEvent.Issue.Number), true);

                if (commentEvent.Issue.PullRequest != null && !string.IsNullOrEmpty(commentEvent.Issue.PullRequest.HtmlUrl))
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on pull request ".t()));
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on issue ".t()));
                    
                blocks.Add(new NewsFeedElement.TextBlock(eventModel.Repo.Name + "#" + commentEvent.Issue.Number, () => action()));
                blocks.Add(new NewsFeedElement.TextBlock(": " + commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * ISSUES EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
            {
                img = Images.Flag;
                var issueEvent = (EventModel.IssuesEvent)eventModel.PayloadObject;
                var action = elementAction = () => NavigationController.PushViewController(new IssueViewController(repoId.Owner, repoId.Name, issueEvent.Issue.Number), true);

                if (string.Equals(issueEvent.Action, "opened", StringComparison.OrdinalIgnoreCase))
                    blocks.Add(new NewsFeedElement.TextBlock("Opened issue ".t()));
                else if (string.Equals(issueEvent.Action, "closed", StringComparison.OrdinalIgnoreCase))
                    blocks.Add(new NewsFeedElement.TextBlock("Closed issue ".t()));
                else if (string.Equals(issueEvent.Action, "reopened", StringComparison.OrdinalIgnoreCase))
                    blocks.Add(new NewsFeedElement.TextBlock("Reopened issue ".t()));

                blocks.Add(new NewsFeedElement.TextBlock(eventModel.Repo.Name + "#" + issueEvent.Issue.Number, () => action()));
                blocks.Add(new NewsFeedElement.TextBlock(": " + issueEvent.Issue.Title.Trim()));
            }
            /*
             * MEMBER EVENT
             */
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
            {
                img = Images.Group;
                var memberEvent = (EventModel.MemberEvent)eventModel.PayloadObject;
                elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(repoId.Owner, repoId.Name), true);

                if (memberEvent.Action.Equals("added"))
                    blocks.Add(new NewsFeedElement.TextBlock("Added as a collaborator".t()));
                else if (memberEvent.Action.Equals("removed"))
                    blocks.Add(new NewsFeedElement.TextBlock("Removed as a collaborator".t()));

                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(" to ".t()));
                    blocks.Add(RepoName(eventModel.Repo));
                }
            }
            /*
             * PUBLIC EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
            {
                img = Images.Heart;
                elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(repoId.Owner, repoId.Name), true);
                if (ReportRepository)
                {
                    blocks.Add(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(" has been open sourced!".t()));
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Has been open sourced!".t()));
            }
            /*
             * PULL REQUEST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
            {
                img = Images.Fork;
                var pullEvent = (EventModel.PullRequestEvent)eventModel.PayloadObject;
                var action = elementAction = () => NavigationController.PushViewController(new PullRequestViewController(repoId.Owner, repoId.Name, pullEvent.Number), true);

                if (pullEvent.Action.Equals("closed"))
                    blocks.Add(new NewsFeedElement.TextBlock("Closed pull request ".t()));
                else if (pullEvent.Action.Equals("opened"))
                    blocks.Add(new NewsFeedElement.TextBlock("Opened pull request ".t()));
                else if (pullEvent.Action.Equals("synchronize"))
                    blocks.Add(new NewsFeedElement.TextBlock("Synchronized pull request ".t()));
                else if (pullEvent.Action.Equals("reopened"))
                    blocks.Add(new NewsFeedElement.TextBlock("Reopened pull request ".t()));

                blocks.Add(new NewsFeedElement.TextBlock(eventModel.Repo.Name + "#" + pullEvent.PullRequest.Number, () => action()));
                blocks.Add(new NewsFeedElement.TextBlock(": " + pullEvent.PullRequest.Title));
            }
            /*
             * PULL REQUEST REVIEW COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
            {
                img = Images.CommentAdd;
                var commentEvent = (EventModel.PullRequestReviewCommentEvent)eventModel.PayloadObject;
                elementAction = () => NavigationController.PushViewController(new PullRequestsViewController(repoId.Owner, repoId.Name), true);
                blocks.Add(new NewsFeedElement.TextBlock("Commented on pull request ".t()));
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(" in ".t()));
                    blocks.Add(RepoName(eventModel.Repo));
                }

                blocks.Add(new NewsFeedElement.TextBlock(": " + commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * PUSH EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PushEvent)
            {
                img = Images.Plus;
                var pushEvent = (EventModel.PushEvent)eventModel.PayloadObject;

                if (eventModel.Repo != null)
                    elementAction = () => NavigationController.PushViewController(new ChangesetInfoViewController(repoId.Owner, repoId.Name, pushEvent.Commits[0].Sha) { Repo = repoId }, true);

                blocks.Add(new NewsFeedElement.TextBlock("Pushed to ".t()));
                if (!string.IsNullOrEmpty(pushEvent.Ref))
                {
                    var lastSlash = pushEvent.Ref.LastIndexOf("/") + 1;
                    blocks.Add(new NewsFeedElement.TextBlock(pushEvent.Ref.Substring(lastSlash), () => NavigationController.PushViewController(new BranchesViewController(repoId.Owner, repoId.Name), true)));
                }

                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(" at ".t()));
                    blocks.Add(RepoName(eventModel.Repo));
                }

                var desc = (pushEvent.Commits[0].Message ?? "").Replace('\n', ' ').Replace("\r", "").Trim();
                blocks.Add(new NewsFeedElement.TextBlock(": " + desc));
            }
            /*
             * TEAM ADD EVENT
             */
            else if (eventModel.PayloadObject is EventModel.TeamAddEvent)
            {
                img = Images.Team;
                var teamEvent = (EventModel.TeamAddEvent)eventModel.PayloadObject;

                if (teamEvent.User != null)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(teamEvent.User.Login, () => NavigationController.PushViewController(new ProfileViewController(teamEvent.User.Login), true)));
                    blocks.Add(new NewsFeedElement.TextBlock(" added to team ".t()));
                }
                else if (teamEvent.Repo != null)
                {
                    blocks.Add(RepoName(new EventModel.RepoModel { Id = teamEvent.Repo.Id, Name = teamEvent.Repo.FullName, Url = teamEvent.Repo.Url }));
                    blocks.Add(new NewsFeedElement.TextBlock(" added to team ".t()));
                }
                else
                    return null;

                if (teamEvent.Team != null)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(teamEvent.Team.Name, () => { }));
                }
            }
            /*
             * WATCH EVENT
             */
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
            {
                var watchEvent = (EventModel.WatchEvent)eventModel.PayloadObject;
                elementAction = () => NavigationController.PushViewController(new RepositoryInfoViewController(repoId.Owner, repoId.Name), true);
                if (watchEvent.Action.Equals("started"))
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Started watching ".t()));
                    img = Images.HeartAdd;
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Stopped watching ".t()));
                    img = Images.HeartDelete;
                }

                blocks.Add(RepoName(eventModel.Repo));
            }

            return blocks;
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


        private NewsFeedElement.TextBlock RepoName(EventModel.RepoModel repoModel)
        {
            //Most likely indicates a deleted repository
            if (repoModel == null)
                return new NewsFeedElement.TextBlock("Unknown Repository");
            if (repoModel.Name == null)
                return new NewsFeedElement.TextBlock("<Deleted Repository>", color: UIColor.Red);

            var repoSplit = repoModel.Name.Split('/');
            if (repoSplit.Length < 2)
                return new NewsFeedElement.TextBlock(repoModel.Name);

            var repoOwner = repoSplit[0];
            var repoName = repoSplit[1];
            if (!repoOwner.ToLower().Equals(Application.Accounts.ActiveAccount.Username.ToLower()))
                return new NewsFeedElement.TextBlock(repoModel.Name, () => RepoTapped(repoModel));

            //Just return the name
            return new NewsFeedElement.TextBlock(repoName, () => RepoTapped(repoModel));
        }

        
        private static bool ValidRepo(EventModel.RepoModel repoModel)
        {
            return (repoModel != null && repoModel.Name != null);
        }

        private void RepoTapped(EventModel.RepoModel e)
        {
            if (e != null && ValidRepo(e))
            {
                var repoNameSplit = e.Name.Split('/');
                NavigationController.PushViewController(new RepositoryInfoViewController(repoNameSplit[0], repoNameSplit[1], repoNameSplit[1]), true);
            }
        }

    }
}