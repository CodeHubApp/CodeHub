using System;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssuesView : BaseIssuesView<IssuesViewModel>
    {
        private readonly IApplicationService _applicationService;
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public IssuesView(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public override void ViewDidLoad()
        {
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => ViewModel.GoToNewIssueCommand.Execute(null));

            base.ViewDidLoad();

            _viewSegment = new CustomUISegmentedControl(new [] { "Open", "Closed", "Mine", "Custom" }, 3);
            _segmentBarButton = new UIBarButtonItem(_viewSegment) {Width = View.Frame.Width - 10f};
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            
            Bind(ViewModel.WhenAnyValue(x => x.Issues), CreateElement);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            //Select which one is currently selected
            if (ViewModel.Filter.Equals(IssuesFilterModel.CreateOpenFilter()))
                _viewSegment.SelectedSegment = 0;
            else if (ViewModel.Filter.Equals(IssuesFilterModel.CreateClosedFilter()))
                _viewSegment.SelectedSegment = 1;
            else if (ViewModel.Filter.Equals(IssuesFilterModel.CreateMineFilter(_applicationService.Account.Username)))
                _viewSegment.SelectedSegment = 2;
            else
                _viewSegment.SelectedSegment = 3;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            // If there is searching going on. Finish it.
            FinishSearch();

            if (_viewSegment.SelectedSegment == 0)
            {
                ViewModel.Filter = IssuesFilterModel.CreateOpenFilter();
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                ViewModel.Filter = IssuesFilterModel.CreateClosedFilter();
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                ViewModel.Filter = IssuesFilterModel.CreateMineFilter(_applicationService.Account.Username);
            }
            else if (_viewSegment.SelectedSegment == 3)
            {
				//ShowFilterController(new IssuesFilterViewController(ViewModel.RepositoryOwner, ViewModel.RepositoryName, ViewModel.Issues));
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        private class CustomUISegmentedControl : UISegmentedControl
        {
            readonly int _multipleTouchIndex;
            public CustomUISegmentedControl(string[] args, int multipleTouchIndex)
                : base(args)
            {
                this._multipleTouchIndex = multipleTouchIndex;
            }

            public override void TouchesEnded(MonoTouch.Foundation.NSSet touches, UIEvent evt)
            {
                var previousSelected = SelectedSegment;
                base.TouchesEnded(touches, evt);
                if (previousSelected == SelectedSegment && SelectedSegment == _multipleTouchIndex)
                    SendActionForControlEvents(UIControlEvent.ValueChanged);
            }
        }
    }
}

