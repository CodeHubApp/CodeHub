using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Notifications;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Activity
{
    public class NotificationsView : BaseTableViewController<NotificationsViewModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread", "Participating", "All" });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);

            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            this.WhenViewModel(x => x.ShownIndex).Subscribe(x => _viewSegment.SelectedSegment = x);
            this.WhenViewModel(x => x.ReadAllCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Images.CheckButton }.WithCommand(x));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;

            var notificationSource = new NotificationTableViewSource(TableView);
            ViewModel.WhenAnyValue(x => x.GroupedNotifications).Where(x => x != null).Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;

            _viewSegment.ValueChanged += (sender, args) => ViewModel.ShownIndex = _viewSegment.SelectedSegment;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ToolbarItems != null && NavigationController != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null && NavigationController != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

