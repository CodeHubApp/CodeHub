using System;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel<IssuesFilterModel>, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToNewIssueCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

	    public IssuesViewModel(IApplicationService applicationService)
	    {
            GoToNewIssueCommand = ReactiveCommand.Create();
	        GoToNewIssueCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<IssueAddViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
                vm.CreatedIssue.Where(x => x != null).Subscribe(x => IssuesCollection.Add(x));
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
	        {
	            var direction = Filter.Ascending ? "asc" : "desc";
	            var state = Filter.Open ? "open" : "closed";
	            var sort = Filter.SortType == BaseIssuesFilterModel.Sort.None ? null : Filter.SortType.ToString().ToLower();
	            var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;
	            var assignee = string.IsNullOrEmpty(Filter.Assignee) ? null : Filter.Assignee;
	            var creator = string.IsNullOrEmpty(Filter.Creator) ? null : Filter.Creator;
	            var mentioned = string.IsNullOrEmpty(Filter.Mentioned) ? null : Filter.Mentioned;
	            var milestone = Filter.Milestone == null ? null : Filter.Milestone.Value;

	            var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues.GetAll(
	                sort: sort, labels: labels, state: state, direction: direction,
	                assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
                return IssuesCollection.SimpleCollectionLoad(request, t as bool?);
	        });
	    }
    }
}

