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

namespace CodeHub.Core.ViewModels.Events
{
    public abstract class BaseEventsViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly IApplicationService ApplicationService;
        private readonly IReactiveCommand<object> _gotoRepositoryCommand;
        private readonly IReactiveCommand<object> _gotoGistCommand;
        private readonly Action<string> _gotoUrlCommand;

        public IReadOnlyReactiveList<EventItemViewModel> Events { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; set; }

        public bool ReportRepository
        {
            get;
            private set;
        }

        protected BaseEventsViewModel(IApplicationService applicationService)
        {
            ApplicationService = applicationService;
            Title = "Events";

            _gotoUrlCommand = new Action<string>(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            var events = new ReactiveList<EventModel>();
            Events = events.CreateDerivedCollection(CreateEventTextBlocks);
            ReportRepository = true;

            _gotoRepositoryCommand = ReactiveCommand.Create();
            _gotoRepositoryCommand.OfType<EventModel.RepoModel>().Subscribe(x =>
            {
                var repoId = new RepositoryIdentifier(x.Name);
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = repoId.Owner;
                vm.RepositoryName = repoId.Name;
                NavigateTo(vm);
            });

            _gotoGistCommand = ReactiveCommand.Create();
            _gotoGistCommand.OfType<EventModel.GistEvent>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Id = x.Gist.Id;
                vm.Gist = x.Gist;
                NavigateTo(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                this.RequestModel(CreateRequest(0, 100), t as bool?, response =>
                {
                    //this.CreateMore(response, m => { }, events.AddRange);
                    events.Reset(response.Data);
                }));
        }
       
        protected abstract GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage);

        private void GoToCommits(EventModel.RepoModel repoModel, string branch)
        {
            var repoId = new RepositoryIdentifier(repoModel.Name);
            var vm = this.CreateViewModel<CommitsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.Branch = branch;
            NavigateTo(vm);
        }

        private void GoToUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            var vm = this.CreateViewModel<UserViewModel>();
            vm.Username = username;
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
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.Id = (int)id;
        }

        private void GoToPullRequest(RepositoryIdentifier repo, int id)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = this.CreateViewModel<PullRequestViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.Id = id;
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
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.Node = sha;
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

                bodyBlocks.Add(new TextBlock(commitCommentEvent.Comment.Body));
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
                        eventAction = () => _gotoRepositoryCommand.Execute(eventModel.Repo);
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
                eventAction = () => _gotoRepositoryCommand.Execute(forkedRepo);
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
                eventAction = () => _gotoRepositoryCommand.Execute(eventModel.Repo);
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
                eventAction = () => _gotoGistCommand.ExecuteIfCan(gistEvent);

                if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" created Gist #"));
                else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" updated Gist #"));
				else if (string.Equals(gistEvent.Action, "fork", StringComparison.OrdinalIgnoreCase))
					headerBlocks.Add(new TextBlock(" forked Gist #"));

                headerBlocks.Add(new AnchorBlock(gistEvent.Gist.Id, eventAction));
                bodyBlocks.Add(new TextBlock(gistEvent.Gist.Description.Replace('\n', ' ').Replace("\r", "").Trim()));
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
					foreach (var page in gollumEvent.Pages)
					{
						var p = page;
						bodyBlocks.Add(new AnchorBlock(page.PageName, () => _gotoUrlCommand(p.HtmlUrl)));
						bodyBlocks.Add(new TextBlock(" - " + page.Action + "\n"));
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

                bodyBlocks.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
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
                bodyBlocks.Add(new TextBlock(issueEvent.Issue.Title.Trim()));
            }
            /*
             * MEMBER EVENT
             */
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
            {
                var memberEvent = (EventModel.MemberEvent)eventModel.PayloadObject;
                eventAction = () => _gotoRepositoryCommand.Execute(eventModel.Repo);

                if (memberEvent.Action.Equals("added"))
                    headerBlocks.Add(new TextBlock(" added as a collaborator"));
                else if (memberEvent.Action.Equals("removed"))
                    headerBlocks.Add(new TextBlock(" removed as a collaborator"));

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
                eventAction = () => _gotoRepositoryCommand.Execute(eventModel.Repo);
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

                bodyBlocks.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
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
					foreach (var commit in pushEvent.Commits)
					{
						var desc = (commit.Message ?? "");
						var sha = commit.Sha;
						var firstNewLine = desc.IndexOf("\n");
						if (firstNewLine <= 0)
							firstNewLine = desc.Length;

						desc = desc.Substring(0, firstNewLine);
						var shortSha = commit.Sha;
						if (shortSha.Length > 6)
							shortSha = shortSha.Substring(0, 6);

						bodyBlocks.Add(new AnchorBlock(shortSha, () => GoToChangeset(repoId, sha)));
						bodyBlocks.Add(new TextBlock(" - " + desc + "\n"));
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
				eventAction = () => _gotoRepositoryCommand.Execute(eventModel.Repo);
                headerBlocks.Add(watchEvent.Action.Equals("started") ? 
					new TextBlock(" starred ") : new TextBlock(" unstarred "));
                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
            }
            else if (eventModel.PayloadObject is EventModel.ReleaseEvent)
			{
                var releaseEvent = (EventModel.ReleaseEvent)eventModel.PayloadObject;
				eventAction = () => _gotoUrlCommand(releaseEvent.Release.HtmlUrl);
				headerBlocks.Add(new TextBlock(" " + releaseEvent.Action + " release " + releaseEvent.Release.Name));
			}

            return new EventItemViewModel(eventModel, headerBlocks, bodyBlocks, eventAction);
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
			return new AnchorBlock(repoModel.Name, () => _gotoRepositoryCommand.Execute(repoModel));
        }
//
//        private interface IEventTypeHandler
//        {
//            Type EventType { get; }
//
//            EventItemViewModel Handle(EventModel e);
//        }
//
//        private abstract class EventTypeHandler<T> : IEventTypeHandler
//        {
//            public EventItemViewModel Handle(EventModel e)
//            {
//                return Handle(e, e.PayloadObject as T);
//            }
//
//            public Type EventType { get { return typeof(T); } }
//
//            protected abstract EventItemViewModel Handle(EventModel eventModel, T payload);
//        }
//
//        private class WatchEventTypeHandler : EventTypeHandler<EventModel.WatchEvent>
//        {
//            protected override EventItemViewModel Handle(EventModel eventModel, EventModel.WatchEvent payload)
//            {
//                Action eventAction = null;
//                var headerBlocks = new List<TextBlock>();
//                var bodyBlocks = new List<TextBlock>();
//                var username = eventModel.Actor != null ? eventModel.Actor.Login : null;
//
//                headerBlocks.Add(new AnchorBlock(username, () => GoToUser(username)));
//                headerBlocks.Add(new TextBlock(payload.Action.Equals("started") ? " starred " : " unstarred "));
//                headerBlocks.Add(CreateRepositoryTextBlock(eventModel.Repo));
//                return new EventItemViewModel(eventModel, headerBlocks, bodyBlocks);
//            }
//        }

            
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