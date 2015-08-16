using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using CodeHub.Core.Utilities;
using System.Reactive;
using System.Linq;

namespace CodeHub.Core.ViewModels.Activity
{
    public abstract class BaseEventsViewModel : BaseViewModel, ILoadableViewModel, IPaginatableViewModel
    {
        protected readonly ISessionService SessionService;

        public IReadOnlyReactiveList<EventItemViewModel> Events { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; set; }

        public bool ReportRepository { get; private set; }

        protected BaseEventsViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;
            Title = "Events";

            var events = new ReactiveList<EventModel>(resetChangeThreshold: 1.0);
            Events = events.CreateDerivedCollection(CreateEventTextBlocks);
            ReportRepository = true;

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                events.SimpleCollectionLoad(CreateRequest(), 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x())));
        }

        private void GoToUrl(string url)
        {
            var vm = this.CreateViewModel<WebBrowserViewModel>();
            vm.Init(url);
            NavigateTo(vm);
        }

        private void GoToRepository(EventModel.RepoModel repo)
        {
            var repoId = new RepositoryIdentifier(repo.Name);
            var vm = this.CreateViewModel<RepositoryViewModel>();
            vm.Init(repoId.Owner, repoId.Name);
            NavigateTo(vm);
        }

        private void GoToGist(EventModel.GistEvent gist)
        {
            var vm = this.CreateViewModel<GistViewModel>();
            vm.Init(gist.Gist.Id);
            //vm.Gist = gist.Gist;
            NavigateTo(vm);
        }
       
        protected abstract GitHubRequest<List<EventModel>> CreateRequest();

        private void GoToCommits(EventModel.RepoModel repoModel, string branch)
        {
            var repoId = new RepositoryIdentifier(repoModel.Name);
            var vm = this.CreateViewModel<CommitsViewModel>();
            NavigateTo(vm.Init(repoId.Owner, repoId.Name, branch));
        }

        private void GoToUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            var vm = this.CreateViewModel<UserViewModel>();
            vm.Init(username);
            NavigateTo(vm);
        }

        private void GoToBranches(RepositoryIdentifier repoId)
        {
            var vm = this.CreateViewModel<BranchesAndTagsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.SelectedFilter = BranchesAndTagsViewModel.ShowIndex.Branches;
            NavigateTo(vm);
        }

        private void GoToTags(EventModel.RepoModel eventModel)
        {
            var repoId = new RepositoryIdentifier(eventModel.Name);
            var vm = this.CreateViewModel<BranchesAndTagsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.SelectedFilter = BranchesAndTagsViewModel.ShowIndex.Tags;
            NavigateTo(vm);
        }

        private void GoToIssue(RepositoryIdentifier repo, long id)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = this.CreateViewModel<IssueViewModel>();
            vm.Init(repo.Owner, repo.Name, (int)id);
            NavigateTo(vm);
        }

        private void GoToPullRequest(RepositoryIdentifier repo, int id)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = this.CreateViewModel<PullRequestViewModel>();
            vm.Init(repo.Owner, repo.Name, id);
            NavigateTo(vm);
        }

        private void GoToPullRequests(RepositoryIdentifier repo)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = this.CreateViewModel<PullRequestsViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            NavigateTo(vm);
        }

        private void GoToChangeset(RepositoryIdentifier repo, string sha)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = this.CreateViewModel<CommitViewModel>();
            vm.Init(repo.Owner, repo.Name, sha);
            NavigateTo(vm);
        }

        private EventItemViewModel CreateEventTextBlocks(EventModel eventModel)
        {
            Action eventAction = null;
            var headerBlocks = new List<TextBlock>();
            var bodyBlocks = new List<TextBlock>();
            var repoId = eventModel.Repo != null ? new RepositoryIdentifier(eventModel.Repo.Name) : new RepositoryIdentifier();
            var username = eventModel.Actor != null ? eventModel.Actor.Login : null;

            // Insert the actor
            headerBlocks.Add(new AnchorBlock(username, () => GoToUser(username)));

            /*
             * COMMIT COMMENT EVENT
             */
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
            {
                var commitCommentEvent = (EventModel.CommitCommentEvent)eventModel.PayloadObject;
				var node = commitCommentEvent.Comment.CommitId.Substring(0, commitCommentEvent.Comment.CommitId.Length > 6 ? 6 : commitCommentEvent.Comment.CommitId.Length);
				eventAction = () => GoToChangeset(repoId, commitCommentEvent.Comment.CommitId);
                headerBlocks.Add(new TextBlock(" commented on commit "));
                headerBlocks.Add(new AnchorBlock(node, eventAction));

                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" in "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

                var desc = CreateShortMessage(commitCommentEvent.Comment.Body);
                if (desc != null)
                    bodyBlocks.Add(new TextBlock(desc));
            }
            /*
             * CREATE EVENT
             */
            else if (eventModel.PayloadObject is EventModel.CreateEvent)
            {
                var createEvent = (EventModel.CreateEvent)eventModel.PayloadObject;
                if (createEvent.RefType.Equals("repository"))
                {
                    if (ReportRepository)
                    {
                        eventAction = () => GoToRepository(eventModel.Repo);
						headerBlocks.Add(new TextBlock(" created repository "));
                        headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                    else
						headerBlocks.Add(new TextBlock(" created this repository"));
                }
                else if (createEvent.RefType.Equals("branch"))
                {
                    eventAction = () => GoToBranches(repoId);
					headerBlocks.Add(new TextBlock(" created branch "));
                    headerBlocks.Add(new AnchorBlock(createEvent.Ref, eventAction));

                    if (ReportRepository)
                    {
                        headerBlocks.Add(new TextBlock(" in "));
                        headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                }
                else if (createEvent.RefType.Equals("tag"))
                {
					eventAction = () => GoToTags(eventModel.Repo);
					headerBlocks.Add(new TextBlock(" created tag "));
                    headerBlocks.Add(new AnchorBlock(createEvent.Ref, eventAction));

                    if (ReportRepository)
                    {
                        headerBlocks.Add(new TextBlock(" in "));
                        headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                }
            }
            /*
             * DELETE EVENT
             */
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
            {
                var deleteEvent = (EventModel.DeleteEvent)eventModel.PayloadObject;
                if (deleteEvent.RefType.Equals("branch"))
                {
                    eventAction = () => GoToBranches(repoId);
					headerBlocks.Add(new TextBlock(" deleted branch "));
                }
                else if (deleteEvent.RefType.Equals("tag"))
                {
					eventAction = () => GoToTags(eventModel.Repo);
					headerBlocks.Add(new TextBlock(" deleted tag "));
                }
                else
                    return null;

                headerBlocks.Add(new AnchorBlock(deleteEvent.Ref, eventAction));
                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" in "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }
            }
            /*
             * FOLLOW EVENT
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
                var followEvent = (EventModel.FollowEvent)eventModel.PayloadObject;
                eventAction = () => GoToUser(followEvent.Target.Login);
				headerBlocks.Add(new TextBlock(" started following "));
                headerBlocks.Add(new AnchorBlock(followEvent.Target.Login, eventAction));
            }
            /*
             * FORK EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
            {
                var forkEvent = (EventModel.ForkEvent)eventModel.PayloadObject;
                var forkedRepo = new EventModel.RepoModel {Id = forkEvent.Forkee.Id, Name = forkEvent.Forkee.FullName, Url = forkEvent.Forkee.Url};
                eventAction = () => GoToRepository(forkedRepo);
				headerBlocks.Add(new TextBlock(" forked "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                headerBlocks.Add(new TextBlock(" to "));
                headerBlocks.Add(CreateRepositoryTextBlock(forkedRepo));
            }
            /*
             * FORK APPLY EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
            {
                var forkEvent = (EventModel.ForkApplyEvent)eventModel.PayloadObject;
                eventAction = () => GoToRepository(eventModel.Repo);
				headerBlocks.Add(new TextBlock(" applied fork to "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                headerBlocks.Add(new TextBlock(" on branch "));
                headerBlocks.Add(new AnchorBlock(forkEvent.Head, () => GoToBranches(repoId)));
            }
            /*
             * GIST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.GistEvent)
            {
                var gistEvent = (EventModel.GistEvent)eventModel.PayloadObject;
                eventAction = () => GoToGist(gistEvent);

                if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" created Gist #"));
                else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" updated Gist #"));
				else if (string.Equals(gistEvent.Action, "fork", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" forked Gist #"));

                headerBlocks.Add(new AnchorBlock(gistEvent.Gist.Id, eventAction));

                var desc = CreateShortMessage(gistEvent.Gist.Description);
                if (desc != null)
                    bodyBlocks.Add(new TextBlock(desc));
            }
            /*
             * GOLLUM EVENT (WIKI)
             */
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
            {
				var gollumEvent = eventModel.PayloadObject as EventModel.GollumEvent;
				headerBlocks.Add(new TextBlock(" modified the wiki in "));
				headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));

				if (gollumEvent != null && gollumEvent.Pages != null)
				{
                    for (var i = 0; i < gollumEvent.Pages.Count; i++)
					{
                        var p = gollumEvent.Pages[i];
                        bodyBlocks.Add(new AnchorBlock(p.PageName, () => GoToUrl(p.HtmlUrl)));

                        var newLine = string.Empty;
                        if (i != gollumEvent.Pages.Count - 1)
                            newLine = Environment.NewLine;

                        bodyBlocks.Add(new TextBlock(" - " + p.Action + newLine));
					}
				}
            }
            /*
             * ISSUE COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
            {
                var commentEvent = (EventModel.IssueCommentEvent)eventModel.PayloadObject;

				if (commentEvent.Issue.PullRequest != null && !string.IsNullOrEmpty(commentEvent.Issue.PullRequest.HtmlUrl))
				{
                    eventAction = () => GoToPullRequest(repoId, (int)commentEvent.Issue.Number);
					headerBlocks.Add(new TextBlock(" commented on pull request "));
				}
				else
				{
					eventAction = () => GoToIssue(repoId, commentEvent.Issue.Number);
					headerBlocks.Add(new TextBlock(" commented on issue "));
				}

                headerBlocks.Add(new AnchorBlock("#" + commentEvent.Issue.Number, eventAction));
                headerBlocks.Add(new TextBlock(" in "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));

                var desc = CreateShortMessage(commentEvent.Comment.Body);
                if (desc != null)
                    bodyBlocks.Add(new TextBlock(desc));
            }
            /*
             * ISSUES EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
            {
                var issueEvent = (EventModel.IssuesEvent)eventModel.PayloadObject;
                eventAction  = () => GoToIssue(repoId, issueEvent.Issue.Number);

                if (string.Equals(issueEvent.Action, "opened", StringComparison.OrdinalIgnoreCase))
                    headerBlocks.Add(new TextBlock(" opened issue "));
                else if (string.Equals(issueEvent.Action, "closed", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" closed issue "));
                else if (string.Equals(issueEvent.Action, "reopened", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" reopened issue "));

                headerBlocks.Add(new AnchorBlock("#" + issueEvent.Issue.Number, eventAction));
                headerBlocks.Add(new TextBlock(" in "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                bodyBlocks.Add(new TextBlock(issueEvent.Issue.Title));
            }
            /*
             * MEMBER EVENT
             */
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
            {
                var memberEvent = (EventModel.MemberEvent)eventModel.PayloadObject;
                eventAction = () => GoToRepository(eventModel.Repo);
                headerBlocks.Add(new TextBlock(memberEvent.Action.Equals("added") ? " added " : " removed "));

                if (memberEvent.Member != null)
                    headerBlocks.Add(new AnchorBlock(memberEvent.Member.Login, () => GoToUser(memberEvent.Member.Login)));
                headerBlocks.Add(new TextBlock(" as a collaborator"));

                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" to "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }
            }
            /*
             * PUBLIC EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
            {
                eventAction = () => GoToRepository(eventModel.Repo);
                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" has open sourced "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }
                else
                    headerBlocks.Add(new TextBlock(" has been open sourced this repository!"));
            }
            /*
             * PULL REQUEST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
            {
                var pullEvent = (EventModel.PullRequestEvent)eventModel.PayloadObject;
                eventAction = () => GoToPullRequest(repoId, (int)pullEvent.Number);

                if (pullEvent.Action.Equals("closed"))
                    headerBlocks.Add(new TextBlock(" closed pull request "));
                else if (pullEvent.Action.Equals("opened"))
                    headerBlocks.Add(new TextBlock(" opened pull request "));
                else if (pullEvent.Action.Equals("synchronize"))
                    headerBlocks.Add(new TextBlock(" synchronized pull request "));
                else if (pullEvent.Action.Equals("reopened"))
                    headerBlocks.Add(new TextBlock(" reopened pull request "));

				headerBlocks.Add(new AnchorBlock("#" + pullEvent.PullRequest.Number, eventAction));
                headerBlocks.Add(new TextBlock(" in "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));

                bodyBlocks.Add(new TextBlock(pullEvent.PullRequest.Title));
            }
            /*
             * PULL REQUEST REVIEW COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
            {
                var commentEvent = (EventModel.PullRequestReviewCommentEvent)eventModel.PayloadObject;
                eventAction = () => GoToPullRequests(repoId);
                headerBlocks.Add(new TextBlock(" commented on pull request "));
                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" in "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

                var desc = CreateShortMessage(commentEvent.Comment.Body);
                if (desc != null)
                    bodyBlocks.Add(new TextBlock(desc));
            }
            /*
             * PUSH EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PushEvent)
            {
                var pushEvent = (EventModel.PushEvent)eventModel.PayloadObject;

				string branchRef = null;
				if (!string.IsNullOrEmpty(pushEvent.Ref))
				{
					var lastSlash = pushEvent.Ref.LastIndexOf("/", StringComparison.Ordinal) + 1;
					branchRef = pushEvent.Ref.Substring(lastSlash);
				}

                if (eventModel.Repo != null)
					eventAction = () => GoToCommits(eventModel.Repo, pushEvent.Ref);

                headerBlocks.Add(new TextBlock(" pushed to "));
				if (branchRef != null)
					headerBlocks.Add(new AnchorBlock(branchRef, () => GoToBranches(repoId)));

                if (ReportRepository)
                {
                    headerBlocks.Add(new TextBlock(" at "));
                    headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

				if (pushEvent.Commits != null)
				{
                    for (var i = 0; i < pushEvent.Commits.Count; i++)
					{
                        var commit = pushEvent.Commits[i];
                        var sha = commit.Sha;
                        var desc = CreateShortMessage(commit.Message ?? "");
						
						var shortSha = commit.Sha;
						if (shortSha.Length > 6)
							shortSha = shortSha.Substring(0, 6);

                        var newLine = string.Empty;
                        if (i != pushEvent.Commits.Count - 1)
                            newLine = Environment.NewLine;

						bodyBlocks.Add(new AnchorBlock(shortSha, () => GoToChangeset(repoId, sha)));

                        if (desc != null)
                            bodyBlocks.Add(new TextBlock(" - " + desc + newLine));
					}
				}
            }
            else if (eventModel.PayloadObject is EventModel.TeamAddEvent)
            {
                var teamAddEvent = (EventModel.TeamAddEvent)eventModel.PayloadObject;
                headerBlocks.Add(new TextBlock(" added "));

                if (teamAddEvent.User != null)
                    headerBlocks.Add(new AnchorBlock(teamAddEvent.User.Login, () => GoToUser(teamAddEvent.User.Login)));
                else if (teamAddEvent.Repo != null)
                    headerBlocks.Add(CreateRepositoryTextBlock(new EventModel.RepoModel { Id = teamAddEvent.Repo.Id, Name = teamAddEvent.Repo.FullName, Url = teamAddEvent.Repo.Url }));
                else
                    return null;

                if (teamAddEvent.Team != null)
                {
                    headerBlocks.Add(new TextBlock(" to team "));
                    headerBlocks.Add(new AnchorBlock(teamAddEvent.Team.Name, () => { }));
                }
            }
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
            {
                var watchEvent = (EventModel.WatchEvent)eventModel.PayloadObject;
                eventAction = () => GoToRepository(eventModel.Repo);
                headerBlocks.Add(watchEvent.Action.Equals("started") ? 
					new TextBlock(" starred ") : new TextBlock(" unstarred "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
            }
            else if (eventModel.PayloadObject is EventModel.ReleaseEvent)
			{
                var releaseEvent = (EventModel.ReleaseEvent)eventModel.PayloadObject;
                eventAction = () => GoToUrl(releaseEvent.Release.HtmlUrl);
				headerBlocks.Add(new TextBlock(" " + releaseEvent.Action + " release " + releaseEvent.Release.Name));
			}

            var vm = new EventItemViewModel(eventModel, headerBlocks, bodyBlocks);
            eventAction.Do(x => vm.GoToCommand.Subscribe(_ => x()));
            return vm;
        }

        private static string CreateShortMessage(string message)
        {
            if (message == null) return null;
            var firstBreak = 0;
            var firstNewLine = message.IndexOf("\n", StringComparison.Ordinal);
            var firstLineReturn = message.IndexOf("\r", StringComparison.Ordinal);
            if (firstNewLine > 0 && firstLineReturn > 0)
                firstBreak = Math.Min(firstNewLine, firstLineReturn);
            else if (firstNewLine > 0 && firstLineReturn <= 0)
                firstBreak = firstNewLine;
            else if (firstNewLine <= 0 && firstLineReturn > 0)
                firstBreak = firstLineReturn;
            return firstBreak > 0 ? message.Substring(0, firstBreak) : message;
        }

        private TextBlock CreateRepositoryTextBlock(EventModel.RepoModel repoModel)
        {
            //Most likely indicates a deleted repository
            if (repoModel == null)
                return new TextBlock("Unknown Repository");
            if (repoModel.Name == null)
                return new TextBlock("<Deleted Repository>");

            var repoSplit = repoModel.Name.Split('/');
            if (repoSplit.Length < 2)
                return new TextBlock(repoModel.Name);

//            var repoOwner = repoSplit[0];
//            var repoName = repoSplit[1];
            return new AnchorBlock(repoModel.Name, () => GoToRepository(repoModel));
        }
            
        public class TextBlock
        {
            public string Text { get; set; }

            public TextBlock()
            {
            }

            public TextBlock(string text)
            {
                Text = text;
            }
        }

        public class AnchorBlock : TextBlock
        {
            public AnchorBlock(string text, Action tapped) : base(text)
            {
                Tapped = tapped;
            }

            public Action Tapped { get; set; }

            public AnchorBlock(Action tapped)
            {
                Tapped = tapped;
            }
        }
    }
}