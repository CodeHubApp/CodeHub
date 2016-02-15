using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
	public class BranchesAndTagsView : ViewModelCollectionDrivenDialogViewController
	{
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

		public override void ViewDidLoad()
		{
			Title = "Source";
			NoItemsText = "No Items";

			base.ViewDidLoad();

            NavigationItem.TitleView = _viewSegment;

			var vm = (BranchesAndTagsViewModel)ViewModel;
            this.BindCollection(vm.Items, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Select(_ => x).BindCommand(vm.GoToSourceCommand);
                return e;
            });
            vm.Bind(x => x.SelectedFilter, true).Subscribe(x => _viewSegment.SelectedSegment = (nint)x);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
            _viewSegment.ValueChanged += SegmentChanged;
		}

        void SegmentChanged (object sender, EventArgs e)
        {
            var vm = (BranchesAndTagsViewModel)ViewModel;
            vm.SelectedFilter = (int)_viewSegment.SelectedSegment;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _viewSegment.ValueChanged -= SegmentChanged;
        }
	}
}

