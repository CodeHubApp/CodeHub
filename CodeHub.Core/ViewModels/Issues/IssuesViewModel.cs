using System;
using System.Threading.Tasks;
using CodeHub.Core.Filters;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel<IssuesFilterModel>
    {
        private readonly IMessageService _messageService;
        private readonly IDisposable _addToken, _editToken;

        public string Username { get; }

        public string Repository { get; }

        public ReactiveCommand<Unit, IssueAddViewModel> NewIssueCommand { get; }

        public IssuesViewModel(
            string username,
            string repository,
            IMessageService messageService)
        {
            Username = username;
            Repository = repository;
            _messageService = messageService;

            NewIssueCommand = ReactiveCommand.Create(
                () => new IssueAddViewModel(Username, Repository));

            _issues = new FilterableCollectionViewModel<Octokit.Issue, IssuesFilterModel>("IssuesViewModel:" + Username + "/" + Repository)
            {
                GroupingFunction = Group
            };

            _issues.WhenAnyValue(x => x.Filter)
                   .Select(_ => Unit.Default)
                   .InvokeReactiveCommand(LoadCommand);

            _addToken = _messageService.Listen<IssueAddMessage>(x =>
            {
                if (x.Issue == null || !DoesIssueBelong(x.Issue))
                    return;
                Issues.Items.Insert(0, x.Issue);
            });

            _editToken = _messageService.Listen<IssueEditMessage>(x =>
            {
                if (x.Issue == null || !DoesIssueBelong(x.Issue))
                    return;

                var item = Issues.Items.FirstOrDefault(y => y.Number == x.Issue.Number);
                if (item == null)
                    return;

                var index = Issues.Items.IndexOf(item);

                using (Issues.DeferRefresh())
                {
                    Issues.Items.RemoveAt(index);
                    Issues.Items.Insert(index, x.Issue);
                }
            });
        }

        protected override Task Load()
        {
            string direction = _issues.Filter.Ascending ? "asc" : "desc";
            string state = _issues.Filter.Open ? "open" : "closed";
            string sort = _issues.Filter.SortType == IssuesFilterModel.Sort.None ? null : _issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(_issues.Filter.Labels) ? null : _issues.Filter.Labels;
            string assignee = string.IsNullOrEmpty(_issues.Filter.Assignee) ? null : _issues.Filter.Assignee;
            string creator = string.IsNullOrEmpty(_issues.Filter.Creator) ? null : _issues.Filter.Creator;
            string mentioned = string.IsNullOrEmpty(_issues.Filter.Mentioned) ? null : _issues.Filter.Mentioned;
            string milestone = _issues.Filter.Milestone == null ? null : _issues.Filter.Milestone.Value;


            // TODO: Pagination!
            //var request = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, 
            //                                                                              assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
            ////return Issues.SimpleCollectionLoad(request);

            return Task.Delay(1);
        }

        public void CreateIssue(Octokit.Issue issue)
        {
            if (!DoesIssueBelong(issue))
                return;
            Issues.Items.Add(issue);
        }

        private bool DoesIssueBelong(Octokit.Issue model)
        {
            if (Issues.Filter == null)
                return true;
            if (Issues.Filter.Open != model.State.StringValue.Equals("open"))
                return false;
            return true;
        }
    }
}

