using System;
using MvvmCross.Platform;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.Views.Filters;

namespace CodeHub.iOS.Views.Issues
{
    public class IssuesView : BaseIssuesView
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public new IssuesViewModel ViewModel
        {
            get { return (IssuesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.RightBarButtonItem = addButton;

            base.ViewDidLoad();

            _viewSegment = new CustomUISegmentedControl(new [] { "Open", "Closed", "Mine", "Custom" }, 3);
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            BindCollection(ViewModel.Issues, CreateElement);

            OnActivation(d =>
            {
                d(addButton.GetClickedObservable().BindCommand(ViewModel.GoToNewIssueCommand));
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            var application = Mvx.Resolve<IApplicationService>();

            //Select which one is currently selected
            if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateOpenFilter()))
                _viewSegment.SelectedSegment = 0;
            else if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateClosedFilter()))
                _viewSegment.SelectedSegment = 1;
            else if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateMineFilter(application.Account.Username)))
                _viewSegment.SelectedSegment = 2;
            else
                _viewSegment.SelectedSegment = 3;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            var application = Mvx.Resolve<IApplicationService>();

            // If there is searching going on. Finish it.
            FinishSearch();

            if (_viewSegment.SelectedSegment == 0)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateOpenFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateClosedFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateMineFilter(application.Account.Username), true);
            }
            else if (_viewSegment.SelectedSegment == 3)
            {
                var filter = new IssuesFilterViewController(ViewModel.Username, ViewModel.Repository, ViewModel.Issues);
                var nav = new UINavigationController(filter);
                PresentViewController(nav, true, null);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _viewSegment.ValueChanged -= SegmentValueChanged;
        }

        private class CustomUISegmentedControl : UISegmentedControl
        {
            readonly int _multipleTouchIndex;
            public CustomUISegmentedControl(object[] args, int multipleTouchIndex)
                : base(args)
            {
                this._multipleTouchIndex = multipleTouchIndex;
            }

            public override void TouchesEnded(Foundation.NSSet touches, UIEvent evt)
            {
                var previousSelected = SelectedSegment;
                base.TouchesEnded(touches, evt);
                if (previousSelected == SelectedSegment && SelectedSegment == _multipleTouchIndex)
                    SendActionForControlEvents(UIControlEvent.ValueChanged);
            }
        }
    }
}

