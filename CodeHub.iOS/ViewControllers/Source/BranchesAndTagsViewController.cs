using System;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesAndTagsViewController : BaseTableViewController<BranchesAndTagsViewModel>
	{
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            NavigationItem.TitleView = _viewSegment;

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            TableView.RegisterClassForCellReuse(typeof(TagCellView), TagCellView.Key);

            OnActivation(d => {
                d(_viewSegment.GetChangedObservable().Subscribe(x => ViewModel.SelectedFilter = (BranchesAndTagsViewModel.ShowIndex)x));
                d(this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => {
                    _viewSegment.SelectedSegment = (int)x;

                    TableView.Source?.Dispose();
                    TableView.Source = null;

                    if (x == BranchesAndTagsViewModel.ShowIndex.Branches)
                    {
                        var source = new ReactiveTableViewSource<BranchItemViewModel>(TableView, ViewModel.Branches, BranchCellView.Key, 44f);
                        source.ElementSelected.OfType<BranchItemViewModel>().Subscribe(y => y.GoToCommand.ExecuteIfCan());
                        TableView.Source = source;
                    }
                    else
                    {
                        var source = new ReactiveTableViewSource<TagItemViewModel>(TableView, ViewModel.Tags, TagCellView.Key, 44f);
                        source.ElementSelected.OfType<TagItemViewModel>().Subscribe(y => y.GoToCommand.ExecuteIfCan());
                        TableView.Source = source;
                    }
                }));
            });
		}
	}
}

