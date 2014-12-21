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
        private readonly IssuesFilterModel _openFilter = IssuesFilterModel.CreateOpenFilter();
        private readonly IssuesFilterModel _closedFilter = IssuesFilterModel.CreateClosedFilter();
        private readonly IssuesFilterModel _mineFilter;

        private readonly IApplicationService _applicationService;
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public IssuesView(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            _mineFilter = IssuesFilterModel.CreateMineFilter(_applicationService.Account.Username);
        }

        public override void ViewDidLoad()
        {
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add).WithCommand(ViewModel.GoToNewIssueCommand);

            base.ViewDidLoad();

            _viewSegment = new CustomUISegmentedControl(new [] { "Open", "Closed", "Mine", "Custom" }, 3);
            _segmentBarButton = new UIBarButtonItem(_viewSegment) {Width = View.Frame.Width - 10f};
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            
//            this.BindList(ViewModel.Issues, CreateElement);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;
            _viewSegment.SelectedSegment = (int)ViewModel.FilterSelection;
            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            switch (_viewSegment.SelectedSegment)
            {
                case 0:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Open;
                    break;
                case 1:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Closed;
                    break;
                case 2:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Mine;
                    break;
                case 3:
                    ViewModel.GoToCustomFilterCommand.ExecuteIfCan();
                    break;
            }
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

