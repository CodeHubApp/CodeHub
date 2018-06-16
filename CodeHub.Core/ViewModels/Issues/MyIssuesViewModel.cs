using System.Threading.Tasks;
using CodeHub.Core.Filters;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel<MyIssuesFilterModel>
    {
        private IDisposable _editToken;

        private int _selectedFilter;
        public int SelectedFilter
        {
            get => _selectedFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilter, value);
        }

        public MyIssuesViewModel(IMessageService messageService = null)
        {
            messageService = messageService ?? GetService<IMessageService>();
            
            _issues = new FilterableCollectionViewModel<Octokit.Issue, MyIssuesFilterModel>("MyIssues");
            _issues.GroupingFunction = Group;

            _issues.WhenAnyValue(x => x.Filter)
                   .Select(_ => Unit.Default)
                   .InvokeReactiveCommand(LoadCommand);

            this.WhenAnyValue(x => x.SelectedFilter).Subscribe(x =>
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

        protected override Task Load()
        {
            string filter = Issues.Filter.FilterType.ToString().ToLower();
            string direction = Issues.Filter.Ascending ? "asc" : "desc";
            string state = Issues.Filter.Open ? "open" : "closed";
            string sort = Issues.Filter.SortType == MyIssuesFilterModel.Sort.None ? null : Issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(Issues.Filter.Labels) ? null : Issues.Filter.Labels;

            //var request = this.GetApplication().Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, filter: filter);
            //return Issues.SimpleCollectionLoad(request);
            return Task.Delay(1);
        }
    }
}

