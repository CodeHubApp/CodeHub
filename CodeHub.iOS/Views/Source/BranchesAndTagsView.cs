using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;
using UIKit;

namespace CodeHub.iOS.Views.Source
{
	public class BranchesAndTagsView : ViewModelCollectionDrivenDialogViewController
	{
		private UISegmentedControl _viewSegment;
		private UIBarButtonItem _segmentBarButton;

		public override void ViewDidLoad()
		{
			Title = "Source".t();
			NoItemsText = "No Items".t();

			base.ViewDidLoad();

			_viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});
			_segmentBarButton = new UIBarButtonItem(_viewSegment) { Width = View.Frame.Width - 10f };
			ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

			var vm = (BranchesAndTagsViewModel)ViewModel;
			this.BindCollection(vm.Items, x => new StyledStringElement(x.Name, () => vm.GoToSourceCommand.Execute(x)));
            _viewSegment.ValueChanged += (sender, e) => vm.SelectedFilter = (int)_viewSegment.SelectedSegment; 
            vm.Bind(x => x.SelectedFilter, x => _viewSegment.SelectedSegment = (nint)x, true);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			NavigationController?.SetToolbarHidden(false, animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			NavigationController?.SetToolbarHidden(true, animated);
		}
	}
}

