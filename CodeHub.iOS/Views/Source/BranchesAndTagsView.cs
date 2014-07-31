using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Source
{
	public class BranchesAndTagsView : ViewModelCollectionViewController<BranchesAndTagsViewModel>
	{
		private UISegmentedControl _viewSegment;
		private UIBarButtonItem _segmentBarButton;

        public BranchesAndTagsView()
        {
            Title = "Source";
            //NoItemsText = "No Items";
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});
			_segmentBarButton = new UIBarButtonItem(_viewSegment) { Width = View.Frame.Width - 10f };
		    _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = (BranchesAndTagsViewModel.ShowIndex) _viewSegment.SelectedSegment;
		    ViewModel.WhenAnyValue(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = (int)x);
            this.BindList(ViewModel.Items, x => new StyledStringElement(x.Name, () => ViewModel.GoToSourceCommand.Execute(x.Object)));

            ToolbarItems = new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			if (ToolbarItems != null)
				NavigationController.SetToolbarHidden(false, animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			if (ToolbarItems != null)
				NavigationController.SetToolbarHidden(true, animated);
		}
	}
}

