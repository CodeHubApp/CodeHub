using System;
using CodeHub.GitHub.Controllers.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch;
using CodeHub.GitHub.Controllers.Gists;
using MonoTouch.Foundation;
using ProfileController = CodeHub.GitHub.Controllers.Accounts.ProfileController;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Events
{
    public class EventsController : BaseModelDrivenController
    {
        protected int _nextPage = 1;
        private LoadMoreElement _loadMore;

        private static UIFont LinkFont = UIFont.BoldSystemFontOfSize(12f);
        private static UIColor LinkColor = UIColor.FromRGB(0, 64, 128);

        public string Username { get; private set; }
        public bool ReportRepository { get; set; }

        public new List<EventModel> Model { get { return (List<EventModel>)base.Model; } }

        public EventsController(string username)
            : base(typeof(List<EventModel>))
        {
            Title = "Events";
            Style = UITableViewStyle.Plain;
            Username = username;
            Root.UnevenRows = true;
            ReportRepository = false;
        }

        protected virtual List<EventModel> OnGetData(int start = 1)
        {
            var response = Application.Client.API.GetEvents(Username, start);
            if (response.Next != null)
                _nextPage = start + 1;
            else
                _nextPage = -1;
            return response.Data;
        }

        private void GetMore()
        {
            this.DoWorkNoHud(() =>
            {
                var data = OnGetData(_nextPage);

                InvokeOnMainThread(() => AddItems(data));

                //Should never happen. Sanity check..
                if (_loadMore != null && _nextPage <= 0)
                {
                    InvokeOnMainThread(() =>
                    {
                        Root.Remove(_loadMore.Parent as Section);
                        _loadMore.Dispose();
                        _loadMore = null;
                    });
                }
            },
            ex => Utilities.ShowAlert("Failure to load!", "Unable to load additional enries because the following error: " + ex.Message),
            () =>
            {
                if (_loadMore != null)
                    _loadMore.Animating = false;
            });
        }

        protected override object OnUpdateModel(bool forced)
        {
            InvokeOnMainThread(() => {
                Root.Clear();
                _nextPage = 1;
            });

            var events = OnGetData();
            return events;
        }

        protected override void OnRender()
        {
            AddItems(Model);
        }

        private IEnumerable<Element> CreateElement(EventModel eventModel)
        {
            var username = eventModel.Actor != null ? eventModel.Actor.Login : null;
            var avatar = eventModel.Actor != null ? eventModel.Actor.AvatarUrl : null;
            //var desc = string.IsNullOrEmpty(eventModel.Description) ? "" : eventModel.Description.Replace("\n", " ").Trim();
            UIImage img = Images.Priority;

            if (eventModel.PayloadObject == null)
                return null;

            if (eventModel.PayloadObject is EventModel.PushEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.PushEvent;
                img = Images.Plus;

                var elements = new List<Element>(obj.Commits.Count);
                obj.Commits.ForEach(x => {
                    var textBlocks = new List<NewsFeedElement.TextBlock>(10);
                    var desc = string.IsNullOrEmpty(x.Message) ? "" : x.Message.ToOneLine().Trim();
                    if (ReportRepository)
                    {
                        textBlocks.Add(new NewsFeedElement.TextBlock("Commited to "));
                        textBlocks.AddRange(RepoName(eventModel.Repo));
                        textBlocks.Add(new NewsFeedElement.TextBlock(": " + desc));
                    }
                    else
                    {
                        textBlocks.Add(new NewsFeedElement.TextBlock("Commited: " + desc));
                    }
                    var el = new NewsFeedElement(username, avatar, eventModel.CreatedAt, textBlocks, img);
                    if (eventModel.Repo != null && eventModel.Repo.Name != null)
                        el.Tapped += () => NavigationController.PushViewController(new Changesets.ChangesetInfoController(username, RepoSlug(eventModel.Repo), x.Sha), true);
                    elements.Add(el);
                });

                return elements;
            }
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.GollumEvent;
                img = Images.Pencil;

                var elements = new List<Element>(obj.Pages.Count);
                obj.Pages.ForEach(x => {
                    var textBlocks = new List<NewsFeedElement.TextBlock>(10);
                    if (ReportRepository)
                    {
                        textBlocks.Add(new NewsFeedElement.TextBlock(x.Action.ToTitleCase() + " wiki page "));
                        textBlocks.Add(CreateWikiBlock(x));
                        textBlocks.Add(new NewsFeedElement.TextBlock(" in "));
                        textBlocks.AddRange(RepoName(eventModel.Repo));
                    }
                    else
                    {
                        textBlocks.Add(new NewsFeedElement.TextBlock(x.Action.ToTitleCase() + " wiki page "));
                        textBlocks.Add(CreateWikiBlock(x));
                    }
                    var el = new NewsFeedElement(username, avatar, eventModel.CreatedAt, textBlocks, img);
                    elements.Add(el);
                });

                return elements;
            }


            //Create the blocks for the collowing events
            var blocks = new List<NewsFeedElement.TextBlock>(10);
            NSAction action = null;

            //These are normal cases!
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.CommitCommentEvent;
                var desc = obj.Comment.Body.Replace("\n", " ").Trim();
                img = Images.CommentAdd;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on commit in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(": " + desc));
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on commit: " + desc));
            }
            else if (eventModel.PayloadObject is EventModel.CreateEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.CreateEvent;
                var s = eventModel.Repo.Name.Split('/');
                var repoOwner = s[0];
                var repoName = s[1];

                if (obj.RefType.Equals("repository"))
                {
                    img = Images.Repo;
                    blocks.Add(new NewsFeedElement.TextBlock("Created new " + obj.RefType + " "));

                    if (ReportRepository)
                    {
                        action = NavigateToRepository(eventModel.Repo);
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
                else if (obj.RefType.Equals("branch"))
                {
                    img = Images.Branch;
                    action = () => NavigationController.PushViewController(new Controllers.Source.SourceController(repoOwner, repoName, obj.Ref), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created new " + obj.RefType + " "));
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Ref, action));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in "));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
                else if (obj.RefType.Equals("tag"))
                {
                    img = Images.Tag;
                    action = () => NavigationController.PushViewController(new Controllers.Source.SourceController(repoOwner, repoName, obj.Ref), true);
                    blocks.Add(new NewsFeedElement.TextBlock("Created new " + obj.RefType + " "));
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Ref, action));

                    if (ReportRepository)
                    {
                        blocks.Add(new NewsFeedElement.TextBlock(" in "));
                        blocks.AddRange(RepoName(eventModel.Repo));
                    }
                }
                else
                    throw new InvalidOperationException("No such create event for: " + obj.RefType);
            }
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.DeleteEvent;
                action = NavigateToRepository(eventModel.Repo);
                img = Images.BinClosed;
                if (ReportRepository)
                {
                    action = NavigateToRepository(eventModel.Repo);
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted " + obj.RefType + " in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Deleted " + obj.RefType));
            }
            else if (eventModel.PayloadObject is EventModel.DownloadEvent)
            {
                img = Images.Create;
                if (ReportRepository)
                {
                    action = NavigateToRepository(eventModel.Repo);
                    blocks.Add(new NewsFeedElement.TextBlock("Created download in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Created download "));
            }
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.FollowEvent;
                img = Images.HeartAdd;
                blocks.Add(new NewsFeedElement.TextBlock("Begun following "));
                blocks.Add(CreateUserBlock(obj.Target.Login));
                action = () => NavigationController.PushViewController(new ProfileController(obj.Target.Login), true);
            }
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.ForkEvent;
                img = Images.Fork;
                var forkee = new EventModel.RepoModel { 
                    Id = obj.Forkee.Id, 
                    Name = obj.Forkee.FullName, 
                    Url = obj.Forkee.Url 
                };

                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Forked "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(" to "));
                    blocks.AddRange(RepoName(forkee));
                    action = NavigateToRepository(forkee);
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Forked from "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    action = NavigateToRepository(forkee);
                }
            }
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
            {
                img = Images.ServerComponents;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Applied patch to fork "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    action = NavigateToRepository(eventModel.Repo);
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Applied patch to fork"));
            }
            else if (eventModel.PayloadObject is EventModel.GistEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.GistEvent;
                img = Images.Script;
                action = () => NavigationController.PushViewController(new GistInfoController(obj.Gist.Id), true);
                var desc = string.IsNullOrEmpty(obj.Gist.Description) ? "Gist " + obj.Gist.Id : obj.Gist.Description;
                blocks.Add(new NewsFeedElement.TextBlock(obj.Action.ToTitleCase() + " Gist: "));
                blocks.Add(new NewsFeedElement.TextBlock(desc, UIFont.BoldSystemFontOfSize(12f), UIColor.FromRGB(0, 64, 128), action));
            }
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.IssueCommentEvent;
                img = Images.CommentAdd;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on issue "));
                    blocks.Add(CreateIssueBlock(obj.Issue));
                    blocks.Add(new NewsFeedElement.TextBlock(" in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(": " + obj.Comment.Body));
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on issue"));
                    blocks.Add(CreateIssueBlock(obj.Issue));
                    blocks.Add(new NewsFeedElement.TextBlock(": " + obj.Comment.Body));
                }
            }
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.IssuesEvent;
                img = Images.Buttons.Flag;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Action.ToTitleCase() + " issue "));
                    blocks.Add(CreateIssueBlock(obj.Issue));
                    blocks.Add(new NewsFeedElement.TextBlock(" in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Action.ToTitleCase() + " issue "));
                    blocks.Add(CreateIssueBlock(obj.Issue));
                }
            }
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.MemberEvent;
                img = Images.Buttons.Person;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Member added to "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(": "));
                    blocks.Add(CreateUserBlock(obj.Member.Login));
                    action = NavigateToRepository(eventModel.Repo);
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Member added: "));
                    blocks.Add(CreateUserBlock(obj.Member.Login));
                    action = () => NavigationController.PushViewController(new ProfileController(obj.Member.Login), true);
                }
            }
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.PublicEvent;
                img = Images.Buttons.Info;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Open sourced "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    action = NavigateToRepository(eventModel.Repo);
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Open sourced this repository!"));
            }
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.PullRequestEvent;
                img = Images.SitemapColor;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Action.ToTitleCase() + " pull request for "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    action = NavigateToRepository(eventModel.Repo);
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock(obj.Action.ToTitleCase() + " pull request"));
            }
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
            {
                var obj = eventModel.PayloadObject as EventModel.PullRequestReviewCommentEvent;
                img = Images.CommentAdd;
                if (ReportRepository)
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on pull request in "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                    blocks.Add(new NewsFeedElement.TextBlock(": " + obj.Comment.Body));
                    action = NavigateToRepository(eventModel.Repo);
                }
                else
                    blocks.Add(new NewsFeedElement.TextBlock("Commented on pull request: " + obj.Comment.Body));
            }
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
            {
                img = Images.Plus;
                if (ReportRepository)
                {
                    action = NavigateToRepository(eventModel.Repo);
                    blocks.Add(new NewsFeedElement.TextBlock("Begun watching: "));
                    blocks.AddRange(RepoName(eventModel.Repo));
                }
                else
                {
                    blocks.Add(new NewsFeedElement.TextBlock("Begun watching this repository"));
                    action = () => NavigationController.PushViewController(new ProfileController(eventModel.Actor.Login), true);

                }
            }

            var element = new NewsFeedElement(username, avatar, eventModel.CreatedAt, blocks, img);
            if (action != null)
                element.Tapped += action;

            return new [] { element };
        }
        
        private void RepoTapped(string owner, string repo)
        {
            if (!string.IsNullOrEmpty(owner) && !string.IsNullOrEmpty(repo))
                NavigationController.PushViewController(new RepositoryInfoController(owner, repo), true);
        }

        private NewsFeedElement.TextBlock CreateUserBlock(string username)
        {
            return new NewsFeedElement.TextBlock(username, () => NavigationController.PushViewController(new ProfileController(username), true));
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
                return () => NavigationController.PushViewController(new Repositories.RepositoryInfoController(repoNameSplit[0], repoNameSplit[1]), true);
            }

            return null;
        }

        private bool ValidRepo(EventModel.RepoModel repoModel)
        {
            return (repoModel != null && repoModel.Name != null);
        }

        private string RepoSlug(EventModel.RepoModel repoModel)
        {
            if (repoModel == null || repoModel.Name == null)
                return null;
            var repoSplit = repoModel.Name.Split('/');
            if (repoSplit.Length < 2)
                return repoModel.Name;
            return repoSplit[1];
        }

        private IEnumerable<NewsFeedElement.TextBlock> RepoName(EventModel.RepoModel repoModel)
        {
            //Most likely indicates a deleted repository
            if (repoModel == null)
                return new [] { new NewsFeedElement.TextBlock("Unknown Repository") };
            if (repoModel.Name == null)
                return new [] { new NewsFeedElement.TextBlock("(Deleted Repository)") };

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
                    new NewsFeedElement.TextBlock(repoOwner, () => NavigationController.PushViewController(new ProfileController(repoOwner), true)),
                    new NewsFeedElement.TextBlock("/", UIFont.BoldSystemFontOfSize(12f)),
                    new NewsFeedElement.TextBlock(repoName, () => RepoTapped(repoOwner, repoName)),
                };
            }

            //Just return the name
            return new [] { new NewsFeedElement.TextBlock(repoName, () => RepoTapped(repoOwner, repoName)) };
        }



        private void AddItems(List<EventModel> events)
        {
            var sec = new Section();
            foreach (var e in events)
            {
                try
                {
                    var elements = CreateElement(e);
                    if (elements != null)
                        sec.AddAll(elements);
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Unable to add event", ex);
                }
            }

            if (sec.Count == 0)
                return;

            if (Root.Count == 0)
            {
                var r = new RootElement(Title) { sec };

                //If there are more items to load then insert the load object
                if (_nextPage > 0)
                {
                    _loadMore = new PaginateElement("Load More", "Loading...", e => GetMore());
                    r.Add(new Section { _loadMore });
                }

                Root = r;
            }
            else
            {
                Root.Insert(Root.Count - 1, sec);
            }
        }
    }
}