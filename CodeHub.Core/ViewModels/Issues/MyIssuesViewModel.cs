using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesViewModel : BaseIssuesViewModel, ILoadableViewModel
    {
        private readonly MyIssuesFilterModel _openFilter = MyIssuesFilterModel.CreateOpenFilter();
        private readonly MyIssuesFilterModel _closedFilter = MyIssuesFilterModel.CreateClosedFilter();

        private readonly ObservableAsPropertyHelper<int> _selectedFilter;
		public int SelectedFilter
		{
            get { return _selectedFilter.Value; }
			set
            {
                switch (value)
                {
                    case 0:
                        Filter = _openFilter;
                        break;
                    case 1:
                        Filter = _closedFilter;
                        break;
                }
            }
		}

        private MyIssuesFilterModel _filter;
        private MyIssuesFilterModel Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public MyIssuesViewModel(IApplicationService applicationService)
        {
            Title = "My Issues";
            Filter = MyIssuesFilterModel.CreateOpenFilter();

            _selectedFilter = this.WhenAnyValue(x => x.Filter)
                .Select(x =>
                {
                    if (x == null || _openFilter.Equals(x))
                        return 0;
                    return _closedFilter.Equals(x) ? 1 : -1;
                })
                .ToProperty(this, x => x.SelectedFilter);

            this.WhenAnyValue(x => x.Filter).Skip(1).Subscribe(LoadCommand.ExecuteIfCan);

            //Filter = applicationService.Account.Filters.GetFilter<MyIssuesFilterModel>("MyIssues");
//            Issues.GroupFunc = x =>
//            {
//                var @group = base.Group(model);
//                if (@group != null) return @group;
//
//                try
//                {
//                    var regex = new System.Text.RegularExpressions.Regex("repos/(.+)/issues/");
//                    return model.GroupBy(x => regex.Match(x.Url).Groups[1].Value).ToList();
//                }
//                catch (Exception e)
//                {
//                    return null;
//                }
//            };



            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var filter = Filter.FilterType.ToString().ToLower();
                var direction = Filter.Ascending ? "asc" : "desc";
                var state = Filter.Open ? "open" : "closed";
                var sort = Filter.SortType == BaseIssuesFilterModel.Sort.None
                    ? null : Filter.SortType.ToString().ToLower();
                var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;

                var request = applicationService.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels,
                    state: state, direction: direction, filter: filter);
                return IssuesCollection.SimpleCollectionLoad(request, t as bool?);
            });
        }
    }
}

