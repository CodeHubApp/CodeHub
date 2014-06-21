using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
	public class MyIssuesViewModel : BaseIssuesViewModel<MyIssuesFilterModel>
    {
		private int _selectedFilter;
		public int SelectedFilter
		{
			get { return _selectedFilter; }
			set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
		}

        public MyIssuesViewModel(IApplicationService applicationService)
        {
            Filter = applicationService.Account.Filters.GetFilter<MyIssuesFilterModel>("MyIssues");
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

            this.WhenAnyValue(x => x.SelectedFilter).Skip(1).Subscribe(x =>
            {
                switch (x)
                {
                    case 0:
                        Filter = MyIssuesFilterModel.CreateOpenFilter();
                        break;
                    case 1:
                        Filter = MyIssuesFilterModel.CreateClosedFilter();
                        break;
                }
            });

            LoadCommand.RegisterAsyncTask(t =>
            {
                var filter = Filter.FilterType.ToString().ToLower();
                var direction = Filter.Ascending ? "asc" : "desc";
                var state = Filter.Open ? "open" : "closed";
                var sort = Filter.SortType == BaseIssuesFilterModel.Sort.None
                    ? null : Filter.SortType.ToString().ToLower();
                var labels = string.IsNullOrEmpty(Filter.Labels) ? null : Filter.Labels;

                var request = applicationService.Client.AuthenticatedUser.Issues.GetAll(sort: sort, labels: labels,
                    state: state, direction: direction, filter: filter);
                return Issues.SimpleCollectionLoad(request, t as bool?);
            });
        }
    }
}

