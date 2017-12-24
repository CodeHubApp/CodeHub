using System.Threading.Tasks;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel<MyIssuesFilterModel>
    {
        private readonly IApplicationService _applicationService;
        private IDisposable _editToken;

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set 
            {
                _selectedFilter = value;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public MyIssuesViewModel(
            IMessageService messageService = null,
            IApplicationService applicationService = null)
        {
            messageService = messageService ?? GetService<IMessageService>();
            _applicationService = applicationService ?? GetService<IApplicationService>();
            
            _issues = new FilterableCollectionViewModel<Octokit.Issue, MyIssuesFilterModel>("MyIssues");
            _issues.GroupingFunction = Group;
            _issues.Bind(x => x.Filter).Subscribe(_ => LoadCommand.Execute(false));

            this.Bind(x => x.SelectedFilter).Subscribe(x =>
            {
                if (x == 0)
                    _issues.Filter = MyIssuesFilterModel.CreateOpenFilter();
                else if (x == 1)
                    _issues.Filter = MyIssuesFilterModel.CreateClosedFilter();
            });

            _editToken = messageService.Listen<IssueEditMessage>(x =>
            {
                if (x.Issue == null)
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

        protected override List<IGrouping<string, Octokit.Issue>> Group(IEnumerable<Octokit.Issue> model)
        {
            var group = base.Group(model);
            if (group == null)
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex("repos/(.+)/issues/");
                    return model.GroupBy(x => regex.Match(x.Url).Groups[1].Value).ToList();
                }
                catch
                {
                    return null;
                }
            }

            return group;
        }

        protected override async Task Load()
        {
            var labels = string.IsNullOrEmpty(Issues.Filter.Labels) ? Enumerable.Empty<string>() : Issues.Filter.Labels.Split(' ');

            var request = new Octokit.IssueRequest
            {
                SortDirection = Issues.Filter.Ascending ? Octokit.SortDirection.Ascending : Octokit.SortDirection.Descending,
                State = Issues.Filter.Open ? Octokit.ItemStateFilter.Open : Octokit.ItemStateFilter.Closed,
                Filter = Issues.Filter.GetIssueFilter(),
                SortProperty = Issues.Filter.GetSort()
            };

            foreach (var label in labels)
                request.Labels.Add(label);

            var issues = await _applicationService.GitHubClient.Issue.GetAllForCurrent();
            Issues.Items.Reset(issues);
        }
    }
}

