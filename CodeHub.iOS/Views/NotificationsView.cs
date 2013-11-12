using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views
{
	public class NotificationsView : ViewModelCollectionDrivenViewController
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public new NotificationsViewModel ViewModel
        {
            get { return (NotificationsViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

		public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread".t(), "Participating".t(), "All".t() });
            _viewSegment.ControlStyle = UISegmentedControlStyle.Bar;
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public override void ViewDidLoad()
        {
            SearchPlaceholder = "Search Notifications".t();
            NoItemsText = "No Notifications".t();
            Title = "Notifications".t();

            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            BindCollection(ViewModel.Notifications, x =>
            {
                var el = new StyledStringElement(x.Subject.Title, x.UpdatedAt.ToDaysAgo(), UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };

                var subject = x.Subject.Type.ToLower();
                if (subject.Equals("issue"))
                    el.Image = Images.Flag;
                else if (subject.Equals("pullrequest"))
                    el.Image = Images.Hand;
                else if (subject.Equals("commit"))
                    el.Image = Images.Commit;

                el.Tapped += () => ViewModel.GoToNotificationCommand.Execute(x);
                return el;
            });
        }
        
        protected override void SearchEnd()
        {
            base.SearchEnd();
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null && !IsSearching)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            //Select which one is currently selected
            if (ViewModel.Notifications.Filter.Equals(NotificationsFilterModel.CreateUnreadFilter()))
                _viewSegment.SelectedSegment = 0;
            else if (ViewModel.Notifications.Filter.Equals(NotificationsFilterModel.CreateParticipatingFilter()))
                _viewSegment.SelectedSegment = 1;
            else
                _viewSegment.SelectedSegment = 2;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                ViewModel.Notifications.ApplyFilter(NotificationsFilterModel.CreateUnreadFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                ViewModel.Notifications.ApplyFilter(NotificationsFilterModel.CreateParticipatingFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                ViewModel.Notifications.ApplyFilter(NotificationsFilterModel.CreateAllFilter(), true);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
            base.ViewWillDisappear(animated);
        }

    }
}

