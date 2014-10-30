using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using Xamarin.Utilities.DialogElements;
using System.Linq;
using System.Collections.Generic;
using GitHubSharp.Models;
using CodeHub.Core.Filters;
using CodeHub.Core.Utilities;
using System;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesView : BaseIssuesView<MyIssuesViewModel>
    {
		private UISegmentedControl _viewSegment;
		private UIBarButtonItem _segmentBarButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			_viewSegment = new UISegmentedControl(new object[] { "Open", "Closed", "Custom" });
			_segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
			ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            TableView.Source = new IssueTableViewSource(TableView, ViewModel.Issues);

//
//
//            ViewModel.Issues.Changed.Select(x => Unit.Default).Merge(
//                ViewModel.WhenAnyValue(x => x.Filter).Select(x => Unit.Default)).Subscribe(_ =>
//                    Root.Reset(ViewModel.Issues.GroupBy(x => x.RepositoryFullName).Select(x => new Section(x.Key) { x.Select(CreateElement) })));
//

//			vm.Bind(x => x.SelectedFilter, x =>
//			{
//				if (x == 2)
//				{
//					ShowFilterController(new CodeHub.iOS.Views.Filters.MyIssuesFilterViewController(vm.Issues));
//				}
//
//                // If there is searching going on. Finish it.
//                FinishSearch();
//			});
        }
//
//
//        protected virtual List<IGrouping<string, IssueModel>> Group(IEnumerable<IssueModel> model)
//        {
//            var order = ViewModel.Filter.SortType;
//            if (order == BaseIssuesFilterModel.Sort.Comments)
//            {
//                var a = ViewModel.Filter.Ascending ? model.OrderBy(x => x.Comments) : model.OrderByDescending(x => x.Comments);
//                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.Comments)).ToList();
//                return FilterGroup.CreateNumberedGroup(g, "Comments");
//            }
//            if (order == BaseIssuesFilterModel.Sort.Updated)
//            {
//                var a = ViewModel.Filter.Ascending ? model.OrderBy(x => x.UpdatedAt) : model.OrderByDescending(x => x.UpdatedAt);
//                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UpdatedAt.TotalDaysAgo()));
//                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Updated");
//            }
//            if (order == BaseIssuesFilterModel.Sort.Created)
//            {
//                var a = ViewModel.Filter.Ascending ? model.OrderBy(x => x.CreatedAt) : model.OrderByDescending(x => x.CreatedAt);
//                var g = a.GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.CreatedAt.TotalDaysAgo()));
//                return FilterGroup.CreateNumberedGroup(g, "Days Ago", "Created");
//            }
//
//            return null;
//        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

