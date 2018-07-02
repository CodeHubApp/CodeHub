using System;
using CodeHub.iOS.DialogElements;
using UIKit;
using System.Collections.Generic;
using CodeHub.Core.Utilities;
using CodeHub.iOS.ViewControllers.Source;
using System.Reactive.Linq;
using CodeHub.Core.Utils;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Events
{
    public class EventsViewController : DialogViewController
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

        public static EventsViewController ForNews()
        {
            return new EventsViewController()
            {
                Title = "News"
            };
        }

        public static EventsViewController ForOrganization(string orgName)
        {
            return new EventsViewController()
            {
                Title = "Events"
            };
        }

        public static EventsViewController ForRepository(string username, string repository)
        {
            return new EventsViewController()
            {
                Title = "Events"
            };
        }

        public static EventsViewController ForUser(string username)
        {
            return new EventsViewController()
            {
                Title = "Events"
            };
        }

        public bool ReportRepository { get; private set; }

        public EventsViewController()
        {
            EnableSearch = false;
            ReportRepository = false;
        }

        //public override void ViewDidLoad()
        //{
        //    base.ViewDidLoad();
        //    TableView.RowHeight = UITableView.AutomaticDimension;
        //    TableView.EstimatedRowHeight = 64f;
        //    BindCollection(ViewModel.Events, CreateElement);
        //}

        //private static Element CreateElement(Tuple<Octokit.ActivityPayload, EventBlock> e)
        //{
        //    try
        //    {
        //        if (e.Item2 == null)
        //            return null;

        //        var imgKey = ChooseImage(e.Item1);
        //        var img = Octicon.Alert;
        //        if (_eventToImage.ContainsKey(imgKey))
        //            img = _eventToImage[imgKey];
                    
        //        var avatar = e.Item1.Actor != null ? e.Item1.Actor.AvatarUrl : null;
        //        var headerBlocks = new List<NewsFeedElement.TextBlock>();
        //        foreach (var h in e.Item2.Header)
        //        {
        //            Action act = null;
        //            var anchorBlock = h as AnchorBlock;
        //            if (anchorBlock != null)
        //                act = anchorBlock.Tapped;
        //            headerBlocks.Add(new NewsFeedElement.TextBlock(h.Text, act));
        //        }

        //        var bodyBlocks = new List<NewsFeedElement.TextBlock>();
        //        foreach (var h in e.Item2.Body)
        //        {
        //            Action act = null;
        //            var anchorBlock = h as AnchorBlock;
        //            if (anchorBlock != null)
        //                act = anchorBlock.Tapped;
        //            var block = new NewsFeedElement.TextBlock(h.Text, act);
        //            bodyBlocks.Add(block);
        //        }

        //        var weakTapped = new WeakReference<Action>(e.Item2.Tapped);
        //        var githubAvatar = new GitHubAvatar(avatar).ToUri(64)?.AbsoluteUri;
        //        return new NewsFeedElement(githubAvatar, e.Item1.CreatedAt, headerBlocks, bodyBlocks, img.ToImage(), () => weakTapped.Get()?.Invoke(), e.Item2.Multilined);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Unable to add event: " + ex.Message);
        //        return null;
        //    }
        //}

        //private static EventType ChooseImage(Octokit.Activity activity)
        //{
        //    switch (activity.Type)
        //    {
        //        case "create":
        //            {
        //                activity.Payload.
        //            }
        //    }


        //    if (eventModel.Type is Octokit.CommitCommentPayload)
        //        return EventType.Comment;

        //    var createEvent = eventModel.Payload as Octokit.Payload
        //    if (createEvent != null)
        //    {
        //        var createModel = createEvent;
        //        if (createModel.RefType.Equals("repository"))
        //            return EventType.Repository;
        //        if (createModel.RefType.Equals("branch"))
        //            return EventType.Branch;
        //        if (createModel.RefType.Equals("tag"))
        //            return EventType.Tag;
        //    }
        //    else if (eventModel.Payload is EventModel.DeleteEvent)
        //        return EventType.Delete;
        //    else if (eventModel.Payload is EventModel.FollowEvent)
        //        return EventType.Follow;
        //    else if (eventModel.Payload is EventModel.ForkEvent)
        //        return EventType.Fork;
        //    else if (eventModel.Payload is EventModel.ForkApplyEvent)
        //        return EventType.Fork;
        //    else if (eventModel.Payload is EventModel.GistEvent)
        //        return EventType.Gist;
        //    else if (eventModel.Payload is EventModel.GollumEvent)
        //        return EventType.Wiki;
        //    else if (eventModel.Payload is EventModel.IssueCommentEvent)
        //        return EventType.Comment;
        //    else if (eventModel.Payload is Octokit.IssueEventPayload)
        //        return EventType.Issue;
        //    else if (eventModel.Payload is EventModel.MemberEvent)
        //        return EventType.Organization;
        //    else if (eventModel.Payload is EventModel.PublicEvent)
        //        return EventType.Public;
        //    else if (eventModel.Payload is EventModel.PullRequestEvent)
        //        return EventType.PullRequest;
        //    else if (eventModel.Payload is EventModel.PullRequestReviewCommentEvent)
        //        return EventType.Comment;
        //    else if (eventModel.Payload is Octokit.PushEventPayload)
        //        return EventType.Commit;
        //    else if (eventModel.Payload is EventModel.TeamAddEvent)
        //        return EventType.Organization;
        //    else if (eventModel.Payload is EventModel.WatchEvent)
        //        return EventType.Star;
        //    else if (eventModel.Payload is EventModel.ReleaseEvent)
        //        return EventType.Tag;
        //    return EventType.Unknown;
        //}


        //private readonly CollectionViewModel<Tuple<EventModel, EventBlock>> _events = new CollectionViewModel<Tuple<EventModel, EventBlock>>();

        //public CollectionViewModel<Tuple<EventModel, EventBlock>> Events => _events;

        //protected override Task Load()
        //{
        //    return this.RequestModel(CreateRequest(0, 100), response => {
        //        this.CreateMore(response, m => Events.MoreItems = m, d => Events.Items.AddRange(CreateDataFromLoad(d)));
        //        Events.Items.Reset(CreateDataFromLoad(response.Data));
        //    });
        //}

        //private IEnumerable<Tuple<EventModel, EventBlock>> CreateDataFromLoad(List<EventModel> events)
        //{
        //    var transformedEvents = new List<Tuple<EventModel, EventBlock>>(events.Count);
        //    foreach (var e in events)
        //    {
        //        try
        //        {
        //            transformedEvents.Add(new Tuple<EventModel, EventBlock>(e, CreateEventTextBlocks(e)));
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine(ex.ToString());
        //        }
        //    }
        //    return transformedEvents;
        //}

        //protected abstract GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage);

        //private void GoToCommits(EventModel.RepoModel repoModel, string branch)
        //{
        //    var repoId = RepositoryIdentifier.FromFullName(repoModel.Name);
        //    if (repoId == null)
        //        return;

        //    this.PushViewController(
        //        CommitsViewController.RepositoryCommits(repoId.Owner, repoId.Name, branch));
        //}

        //public ReactiveCommand<RepositoryIdentifier, RepositoryViewModel> GoToRepositoryCommand { get; }

        //private void GoToRepository(EventModel.RepoModel eventModel)
        //{
        //    var repoId = RepositoryIdentifier.FromFullName(eventModel.Name);
        //    if (repoId != null)
        //        GoToRepositoryCommand.ExecuteNow(repoId);
        //}

        //private void GoToUser(string username)
        //{
        //    if (string.IsNullOrEmpty(username))
        //        return;
        //    ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = username });
        //}

        //private void GoToBranch(RepositoryIdentifier repoId, string branchName)
        //{
        //    if (repoId == null)
        //        return;

        //    var viewController = new SourceTreeViewController(
        //        repoId.Owner, repoId.Name, null, branchName, Utilities.ShaType.Branch);
        //    this.PushViewController(viewController);
        //}

        //private void GoToTag(EventModel.RepoModel eventModel, string tagName)
        //{
        //    var repoId = RepositoryIdentifier.FromFullName(eventModel.Name);
        //    if (repoId == null)
        //        return;

        //    var viewController = new SourceTreeViewController(
        //        repoId.Owner, repoId.Name, null, tagName, Utilities.ShaType.Tag);
        //    this.PushViewController(viewController);
        //}

        //public ICommand GoToGistCommand
        //{
        //    get { return new MvxCommand<EventModel.GistEvent>(x => ShowViewModel<GistViewModel>(new GistViewModel.NavObject { Id = x.Gist.Id }), x => x != null && x.Gist != null); }
        //}

        //private void GoToIssue(RepositoryIdentifier repo, long id)
        //{
        //    if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
        //        return;

        //    ShowViewModel<IssueViewModel>(new IssueViewModel.NavObject
        //    {
        //        Username = repo.Owner,
        //        Repository = repo.Name,
        //        Id = id
        //    });
        //}

        //private void GoToPullRequest(RepositoryIdentifier repo, long id)
        //{
        //    if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
        //        return;
        //    ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject
        //    {
        //        Username = repo.Owner,
        //        Repository = repo.Name,
        //        Id = id
        //    });
        //}

        //private void GoToPullRequests(RepositoryIdentifier repo)
        //{
        //    if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
        //        return;
        //    ShowViewModel<PullRequestsViewModel>(new PullRequestsViewModel.NavObject
        //    {
        //        Username = repo.Owner,
        //        Repository = repo.Name
        //    });
        //}

        //private void GoToChangeset(RepositoryIdentifier repo, string sha)
        //{
        //    if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
        //        return;
        //    ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject
        //    {
        //        Username = repo.Owner,
        //        Repository = repo.Name,
        //        Node = sha
        //    });
        //}

        //private EventBlock CreateEventTextBlocks(EventModel eventModel)
        //{
        //    var eventBlock = new EventBlock();
        //    var repoId = eventModel.Repo != null ? RepositoryIdentifier.FromFullName(eventModel.Repo.Name)
        //        : new RepositoryIdentifier(string.Empty, string.Empty);
        //    var username = eventModel.Actor != null ? eventModel.Actor.Login : null;

        //    // Insert the actor
        //    eventBlock.Header.Add(new AnchorBlock(username, () => GoToUser(username)));

        //    var commitCommentEvent = eventModel.PayloadObject as EventModel.CommitCommentEvent;
        //    if (commitCommentEvent != null)
        //    {
        //        var node = commitCommentEvent.Comment.CommitId.Substring(0, commitCommentEvent.Comment.CommitId.Length > 6 ? 6 : commitCommentEvent.Comment.CommitId.Length);
        //        eventBlock.Tapped = () => GoToChangeset(repoId, commitCommentEvent.Comment.CommitId);
        //        eventBlock.Header.Add(new TextBlock(" commented on commit "));
        //        eventBlock.Header.Add(new AnchorBlock(node, eventBlock.Tapped));

        //        if (ReportRepository)
        //        {
        //            eventBlock.Header.Add(new TextBlock(" in "));
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        }

        //        eventBlock.Body.Add(new TextBlock(commitCommentEvent.Comment.Body));
        //        return eventBlock;
        //    }

        //    var createEvent = eventModel.PayloadObject as EventModel.CreateEvent;
        //    if (createEvent != null)
        //    {
        //        if (createEvent.RefType.Equals("repository"))
        //        {
        //            if (ReportRepository)
        //            {
        //                eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
        //                eventBlock.Header.Add(new TextBlock(" created repository "));
        //                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //            }
        //            else
        //                eventBlock.Header.Add(new TextBlock(" created this repository"));
        //        }
        //        else if (createEvent.RefType.Equals("branch"))
        //        {
        //            eventBlock.Tapped = () => GoToBranch(repoId, createEvent.Ref);
        //            eventBlock.Header.Add(new TextBlock(" created branch "));
        //            eventBlock.Header.Add(new AnchorBlock(createEvent.Ref, eventBlock.Tapped));

        //            if (ReportRepository)
        //            {
        //                eventBlock.Header.Add(new TextBlock(" in "));
        //                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //            }
        //        }
        //        else if (createEvent.RefType.Equals("tag"))
        //        {
        //            eventBlock.Tapped = () => GoToTag(eventModel.Repo, createEvent.Ref);
        //            eventBlock.Header.Add(new TextBlock(" created tag "));
        //            eventBlock.Header.Add(new AnchorBlock(createEvent.Ref, eventBlock.Tapped));

        //            if (ReportRepository)
        //            {
        //                eventBlock.Header.Add(new TextBlock(" in "));
        //                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //            }
        //        }
        //    }


        //    var deleteEvent = eventModel.PayloadObject as EventModel.DeleteEvent;
        //    if (deleteEvent != null)
        //    {
        //        if (deleteEvent.RefType.Equals("branch"))
        //        {
        //            eventBlock.Tapped = () => GoToRepository(eventModel.Repo);
        //            eventBlock.Header.Add(new TextBlock(" deleted branch "));
        //        }
        //        else if (deleteEvent.RefType.Equals("tag"))
        //        {
        //            eventBlock.Tapped = () => GoToRepository(eventModel.Repo);
        //            eventBlock.Header.Add(new TextBlock(" deleted tag "));
        //        }
        //        else
        //            return null;

        //        eventBlock.Header.Add(new AnchorBlock(deleteEvent.Ref, eventBlock.Tapped));
        //        if (!ReportRepository) return eventBlock;
        //        eventBlock.Header.Add(new TextBlock(" in "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        return eventBlock;
        //    }


        //    if (eventModel.PayloadObject is EventModel.DownloadEvent)
        //    {
        //        // Don't show the download event for now...
        //        return null;
        //    }


        //    var followEvent = eventModel.PayloadObject as EventModel.FollowEvent;
        //    if (followEvent != null)
        //    {
        //        eventBlock.Tapped = () => GoToUser(followEvent.Target.Login);
        //        eventBlock.Header.Add(new TextBlock(" started following "));
        //        eventBlock.Header.Add(new AnchorBlock(followEvent.Target.Login, eventBlock.Tapped));
        //        return eventBlock;
        //    }
        //    /*
        //     * FORK EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.ForkEvent)
        //    {
        //        var forkEvent = (EventModel.ForkEvent)eventModel.PayloadObject;
        //        var forkedRepo = new EventModel.RepoModel { Id = forkEvent.Forkee.Id, Name = forkEvent.Forkee.FullName, Url = forkEvent.Forkee.Url };
        //        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(forkedRepo);
        //        eventBlock.Header.Add(new TextBlock(" forked "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        eventBlock.Header.Add(new TextBlock(" to "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(forkedRepo));
        //    }
        //    /*
        //     * FORK APPLY EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
        //    {
        //        var forkEvent = (EventModel.ForkApplyEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
        //        eventBlock.Header.Add(new TextBlock(" applied fork to "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        eventBlock.Header.Add(new TextBlock(" on branch "));
        //        eventBlock.Header.Add(new AnchorBlock(forkEvent.Head, () => GoToBranch(repoId, forkEvent.Head)));
        //    }
        //    /*
        //     * GIST EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.GistEvent)
        //    {
        //        var gistEvent = (EventModel.GistEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToGistCommand.Execute(gistEvent);

        //        if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" created Gist #"));
        //        else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" updated Gist #"));
        //        else if (string.Equals(gistEvent.Action, "fork", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" forked Gist #"));

        //        eventBlock.Header.Add(new AnchorBlock(gistEvent.Gist.Id, eventBlock.Tapped));
        //        eventBlock.Body.Add(new TextBlock(gistEvent.Gist.Description.Replace('\n', ' ').Replace("\r", "").Trim()));
        //    }
        //    /*
        //     * GOLLUM EVENT (WIKI)
        //     */
        //    else if (eventModel.PayloadObject is EventModel.GollumEvent)
        //    {
        //        var gollumEvent = eventModel.PayloadObject as EventModel.GollumEvent;
        //        eventBlock.Header.Add(new TextBlock(" modified the wiki in "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

        //        if (gollumEvent != null && gollumEvent.Pages != null)
        //        {
        //            foreach (var page in gollumEvent.Pages)
        //            {
        //                var p = page;
        //                eventBlock.Body.Add(new AnchorBlock(page.PageName, () => GoToUrlCommand.Execute(p.HtmlUrl)));
        //                eventBlock.Body.Add(new TextBlock(" - " + page.Action + "\n"));
        //            }

        //            eventBlock.Multilined = true;
        //        }
        //    }
        //    /*
        //     * ISSUE COMMENT EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
        //    {
        //        var commentEvent = (EventModel.IssueCommentEvent)eventModel.PayloadObject;

        //        if (commentEvent.Issue.PullRequest != null && !string.IsNullOrEmpty(commentEvent.Issue.PullRequest.HtmlUrl))
        //        {
        //            eventBlock.Tapped = () => GoToPullRequest(repoId, commentEvent.Issue.Number);
        //            eventBlock.Header.Add(new TextBlock(" commented on pull request "));
        //        }
        //        else
        //        {
        //            eventBlock.Tapped = () => GoToIssue(repoId, commentEvent.Issue.Number);
        //            eventBlock.Header.Add(new TextBlock(" commented on issue "));
        //        }

        //        eventBlock.Header.Add(new AnchorBlock("#" + commentEvent.Issue.Number, eventBlock.Tapped));
        //        eventBlock.Header.Add(new TextBlock(" in "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

        //        eventBlock.Body.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
        //    }
        //    /*
        //     * ISSUES EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.IssuesEvent)
        //    {
        //        var issueEvent = (EventModel.IssuesEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToIssue(repoId, issueEvent.Issue.Number);

        //        if (string.Equals(issueEvent.Action, "opened", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" opened issue "));
        //        else if (string.Equals(issueEvent.Action, "closed", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" closed issue "));
        //        else if (string.Equals(issueEvent.Action, "reopened", StringComparison.OrdinalIgnoreCase))
        //            eventBlock.Header.Add(new TextBlock(" reopened issue "));

        //        eventBlock.Header.Add(new AnchorBlock("#" + issueEvent.Issue.Number, eventBlock.Tapped));
        //        eventBlock.Header.Add(new TextBlock(" in "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        eventBlock.Body.Add(new TextBlock(issueEvent.Issue.Title.Trim()));
        //    }
        //    /*
        //     * MEMBER EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.MemberEvent)
        //    {
        //        var memberEvent = (EventModel.MemberEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);

        //        if (memberEvent.Action.Equals("added"))
        //            eventBlock.Header.Add(new TextBlock(" added "));
        //        else if (memberEvent.Action.Equals("removed"))
        //            eventBlock.Header.Add(new TextBlock(" removed "));

        //        var memberName = memberEvent.Member?.Login;
        //        if (memberName != null)
        //            eventBlock.Header.Add(new AnchorBlock(memberName, () => GoToUser(memberName)));
        //        else
        //            eventBlock.Header.Add(new TextBlock("<deleted user>"));

        //        eventBlock.Header.Add(new TextBlock(" as a collaborator"));

        //        if (ReportRepository)
        //        {
        //            eventBlock.Header.Add(new TextBlock(" to "));
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        }
        //    }
        //    /*
        //     * PUBLIC EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.PublicEvent)
        //    {
        //        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
        //        if (ReportRepository)
        //        {
        //            eventBlock.Header.Add(new TextBlock(" has open sourced "));
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        }
        //        else
        //            eventBlock.Header.Add(new TextBlock(" has been open sourced this repository!"));
        //    }
        //    /*
        //     * PULL REQUEST EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
        //    {
        //        var pullEvent = (EventModel.PullRequestEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToPullRequest(repoId, pullEvent.Number);

        //        if (pullEvent.Action.Equals("closed"))
        //            eventBlock.Header.Add(new TextBlock(" closed pull request "));
        //        else if (pullEvent.Action.Equals("opened"))
        //            eventBlock.Header.Add(new TextBlock(" opened pull request "));
        //        else if (pullEvent.Action.Equals("synchronize"))
        //            eventBlock.Header.Add(new TextBlock(" synchronized pull request "));
        //        else if (pullEvent.Action.Equals("reopened"))
        //            eventBlock.Header.Add(new TextBlock(" reopened pull request "));

        //        eventBlock.Header.Add(new AnchorBlock("#" + pullEvent.PullRequest.Number, eventBlock.Tapped));
        //        eventBlock.Header.Add(new TextBlock(" in "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

        //        eventBlock.Body.Add(new TextBlock(pullEvent.PullRequest.Title));
        //    }
        //    /*
        //     * PULL REQUEST REVIEW COMMENT EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
        //    {
        //        var commentEvent = (EventModel.PullRequestReviewCommentEvent)eventModel.PayloadObject;
        //        eventBlock.Tapped = () => GoToPullRequests(repoId);
        //        eventBlock.Header.Add(new TextBlock(" commented on pull request "));
        //        if (ReportRepository)
        //        {
        //            eventBlock.Header.Add(new TextBlock(" in "));
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        }

        //        eventBlock.Body.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
        //    }
        //    /*
        //     * PUSH EVENT
        //     */
        //    else if (eventModel.PayloadObject is EventModel.PushEvent)
        //    {
        //        var pushEvent = (EventModel.PushEvent)eventModel.PayloadObject;

        //        string branchRef = null;
        //        if (!string.IsNullOrEmpty(pushEvent.Ref))
        //        {
        //            var lastSlash = pushEvent.Ref.LastIndexOf("/", StringComparison.Ordinal) + 1;
        //            branchRef = pushEvent.Ref.Substring(lastSlash);
        //        }

        //        if (eventModel.Repo != null)
        //            eventBlock.Tapped = () => GoToCommits(eventModel.Repo, pushEvent.Ref);

        //        eventBlock.Header.Add(new TextBlock(" pushed to "));
        //        if (branchRef != null)
        //            eventBlock.Header.Add(new AnchorBlock(branchRef, () => GoToBranch(repoId, branchRef)));

        //        if (ReportRepository)
        //        {
        //            eventBlock.Header.Add(new TextBlock(" at "));
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        }

        //        if (pushEvent.Commits != null)
        //        {
        //            foreach (var commit in pushEvent.Commits)
        //            {
        //                var desc = (commit.Message ?? "");
        //                var sha = commit.Sha;
        //                var firstNewLine = desc.IndexOf("\n");
        //                if (firstNewLine <= 0)
        //                    firstNewLine = desc.Length;

        //                desc = desc.Substring(0, firstNewLine);
        //                var shortSha = commit.Sha;
        //                if (shortSha.Length > 6)
        //                    shortSha = shortSha.Substring(0, 6);

        //                eventBlock.Body.Add(new AnchorBlock(shortSha, () => GoToChangeset(repoId, sha)));
        //                eventBlock.Body.Add(new TextBlock(" - " + desc + "\n"));
        //                eventBlock.Multilined = true;
        //            }
        //        }
        //    }


        //    var teamAddEvent = eventModel.PayloadObject as EventModel.TeamAddEvent;
        //    if (teamAddEvent != null)
        //    {
        //        eventBlock.Header.Add(new TextBlock(" added "));

        //        if (teamAddEvent.User != null)
        //            eventBlock.Header.Add(new AnchorBlock(teamAddEvent.User.Login, () => GoToUser(teamAddEvent.User.Login)));
        //        else if (teamAddEvent.Repo != null)
        //            eventBlock.Header.Add(CreateRepositoryTextBlock(new EventModel.RepoModel { Id = teamAddEvent.Repo.Id, Name = teamAddEvent.Repo.FullName, Url = teamAddEvent.Repo.Url }));
        //        else
        //            return null;

        //        if (teamAddEvent.Team == null) return eventBlock;
        //        eventBlock.Header.Add(new TextBlock(" to team "));
        //        eventBlock.Header.Add(new AnchorBlock(teamAddEvent.Team.Name, () => { }));
        //        return eventBlock;
        //    }


        //    var watchEvent = eventModel.PayloadObject as EventModel.WatchEvent;
        //    if (watchEvent != null)
        //    {
        //        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
        //        eventBlock.Header.Add(watchEvent.Action.Equals("started") ?
        //            new TextBlock(" starred ") : new TextBlock(" unstarred "));
        //        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
        //        return eventBlock;
        //    }

        //    var releaseEvent = eventModel.PayloadObject as EventModel.ReleaseEvent;
        //    if (releaseEvent != null)
        //    {
        //        eventBlock.Tapped = () => GoToUrlCommand.Execute(releaseEvent.Release.HtmlUrl);
        //        eventBlock.Header.Add(new TextBlock(" " + releaseEvent.Action + " release " + releaseEvent.Release.Name));
        //        return eventBlock;
        //    }

        //    return eventBlock;
        //}

        //private TextBlock CreateRepositoryTextBlock(EventModel.RepoModel repoModel)
        //{
        //    //Most likely indicates a deleted repository
        //    if (repoModel == null)
        //        return new TextBlock("Unknown Repository");
        //    if (repoModel.Name == null)
        //        return new TextBlock("<Deleted Repository>");

        //    var repoSplit = repoModel.Name.Split('/');
        //    if (repoSplit.Length < 2)
        //        return new TextBlock(repoModel.Name);

        //    //            var repoOwner = repoSplit[0];
        //    //            var repoName = repoSplit[1];
        //    return new AnchorBlock(repoModel.Name, () => GoToRepositoryCommand.Execute(repoModel));
        //}


        //public class EventBlock
        //{
        //    public IList<TextBlock> Header { get; private set; }
        //    public IList<TextBlock> Body { get; private set; }
        //    public Action Tapped { get; set; }
        //    public bool Multilined { get; set; }

        //    public EventBlock()
        //    {
        //        Header = new List<TextBlock>(6);
        //        Body = new List<TextBlock>();
        //    }
        //}

        //public class TextBlock
        //{
        //    public string Text { get; set; }

        //    public TextBlock()
        //    {
        //    }

        //    public TextBlock(string text)
        //    {
        //        Text = text;
        //    }
        //}

        //public class AnchorBlock : TextBlock
        //{
        //    public AnchorBlock(string text, Action tapped) : base(text)
        //    {
        //        Tapped = tapped;
        //    }

        //    public Action Tapped { get; set; }

        //    public AnchorBlock(Action tapped)
        //    {
        //        Tapped = tapped;
        //    }
        //}
    }
}