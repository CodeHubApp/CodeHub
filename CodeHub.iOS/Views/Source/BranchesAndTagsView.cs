using System;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using GitHubSharp.Models;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Source
{
    public class BranchesAndTagsView : ReactiveTableViewController<BranchesAndTagsViewModel>
	{
		private UISegmentedControl _viewSegment;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.AddSearchBar(x => ViewModel.SearchKeyword = x);

            _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});
            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = (BranchesAndTagsViewModel.ShowIndex) _viewSegment.SelectedSegment;
            ViewModel.WhenAnyValue(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = (int)x);
            NavigationItem.TitleView = _viewSegment;

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            TableView.RegisterClassForCellReuse(typeof(TagCellView), TagCellView.Key);

            var source = new ReactiveTableViewSource<object>(TableView);
            var section = new TableSectionInformation<object, UITableViewCell>(ViewModel.Items, SelectCell, 44f);
            TableView.Source = source;
            source.ElementSelected.Subscribe(ViewModel.GoToSourceCommand.ExecuteIfCan);
            source.Data = new [] { section };

            ViewModel.LoadCommand.ExecuteIfCan();
		}

        private static NSString SelectCell(object x)
        {
            if (x is TagModel)
                return TagCellView.Key;
            if (x is BranchModel)
                return BranchCellView.Key;
            return null;
        }
	}
}

