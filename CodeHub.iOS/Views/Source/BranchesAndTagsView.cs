using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Source
{
	public class BranchesAndTagsView : ViewModelCollectionDrivenViewController
	{
		private readonly UISegmentedControl _viewSegment;
		private readonly UIBarButtonItem _segmentBarButton;

		public new BranchesAndTagsViewModel ViewModel
		{
			get { return (BranchesAndTagsViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BranchesAndTagsView()
		{
			_viewSegment = new UISegmentedControl(new object[] {"Branches".t(), "Tags".t()});
			_segmentBarButton = new UIBarButtonItem(_viewSegment);
		}

		public override void ViewDidLoad()
		{
			Title = "Source".t();
			NoItemsText = "No Items".t();

			base.ViewDidLoad();

			_segmentBarButton.Width = View.Frame.Width - 10f;
			ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
			this.BindCollection(ViewModel.Items, x => new StyledStringElement(x.Name, () => ViewModel.GoToSourceCommand.Execute(x)));
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			if (ToolbarItems != null)
				NavigationController.SetToolbarHidden(false, animated);

			//Before we select which one, make sure we detach the event handler or silly things will happen
			_viewSegment.ValueChanged -= SegmentValueChanged;

			//Select which one is currently selected
			if (ViewModel.IsBranchesShowing)
				_viewSegment.SelectedSegment = 0;
			else
				_viewSegment.SelectedSegment = 2;

			_viewSegment.ValueChanged += SegmentValueChanged;
		}

		void SegmentValueChanged (object sender, EventArgs e)
		{
			if (_viewSegment.SelectedSegment == 0)
			{
				ViewModel.ShowBranchesCommand.Execute(null);
			}
			else if (_viewSegment.SelectedSegment == 1)
			{
				ViewModel.ShowTagsCommand.Execute(null);
			}
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			if (ToolbarItems != null)
				NavigationController.SetToolbarHidden(true, animated);
		}
	}
}

