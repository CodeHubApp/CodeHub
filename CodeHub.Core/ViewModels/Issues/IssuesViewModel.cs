using System;
using System.Threading.Tasks;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel<IssuesFilterModel>
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;
        private IDisposable _addToken, _editToken;

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public ICommand GoToNewIssueCommand
        {
            get { return new MvxCommand(() => ShowViewModel<IssueAddViewModel>(new IssueAddViewModel.NavObject { Username = Username, Repository = Repository })); }
        }

        public IssuesViewModel(IMessageService messageService, IApplicationService applicationService)
        {
            _messageService = messageService;
            _applicationService = applicationService;;
        }

        public void Init(NavObject nav)
        {
            Username = nav.Username;
            Repository = nav.Repository;
            _issues = new FilterableCollectionViewModel<Octokit.Issue, IssuesFilterModel>("IssuesViewModel:" + Username + "/" + Repository);
            _issues.GroupingFunction = Group;
            _issues.Bind(x => x.Filter).Subscribe(_ => LoadCommand.Execute(true));

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

        protected override async Task Load()
        {
            string sort = _issues.Filter.SortType == IssuesFilterModel.Sort.None ? null : _issues.Filter.SortType.ToString().ToLower();

            var request = new Octokit.RepositoryIssueRequest();
            request.Assignee = string.IsNullOrEmpty(_issues.Filter.Assignee) ? null : _issues.Filter.Assignee;
            request.SortDirection = _issues.Filter.Ascending ? Octokit.SortDirection.Ascending : Octokit.SortDirection.Descending;
            request.State = _issues.Filter.Open ? Octokit.ItemStateFilter.Open : Octokit.ItemStateFilter.Closed;
            request.Milestone = _issues.Filter.Milestone?.Value;
            request.Mentioned = string.IsNullOrEmpty(_issues.Filter.Mentioned) ? null : _issues.Filter.Mentioned;
            request.Creator = string.IsNullOrEmpty(_issues.Filter.Creator) ? null : _issues.Filter.Creator;

            var labels = string.IsNullOrEmpty(_issues.Filter.Labels) ? Enumerable.Empty<string>() : _issues.Filter.Labels.Split(' ');
            foreach (var label in labels)
                request.Labels.Add(label);

            if (_issues.Filter.SortType != IssuesFilterModel.Sort.None)
            {
                if (_issues.Filter.SortType == BaseIssuesFilterModel<IssuesFilterModel>.Sort.Comments)
                    request.SortProperty = Octokit.IssueSort.Comments;
                else if (_issues.Filter.SortType == BaseIssuesFilterModel<IssuesFilterModel>.Sort.Created)
                    request.SortProperty = Octokit.IssueSort.Created;
                else if (_issues.Filter.SortType == BaseIssuesFilterModel<IssuesFilterModel>.Sort.Updated)
                    request.SortProperty = Octokit.IssueSort.Updated;
            }

            var issues = await _applicationService.GitHubClient.Issue.GetAllForRepository(Username, Repository, request);
            Issues.Items.Reset(issues);
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
            if (Issues.Filter.Open != model.State.Equals("open"))
                return false;
            return true;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

