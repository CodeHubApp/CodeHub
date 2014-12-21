using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Notifications;
using CodeHub.iOS.TableViewSources;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Notifications
{
    public class NotificationsView : ReactiveTableViewController<NotificationsViewModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread", "Participating", "All" });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);

            this.WhenViewModel(x => x.ShownIndex).Subscribe(x => _viewSegment.SelectedSegment = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Theme.CurrentTheme.CheckButton }.WithCommand(ViewModel.ReadAllCommand);

            var notificationSource = new NotificationTableViewSource(TableView);
            notificationSource.ElementSelected.OfType<NotificationModel>().Subscribe(x =>
                ViewModel.GoToNotificationCommand.ExecuteIfCan(x));
            ViewModel.WhenAnyValue(x => x.GroupedNotifications).Where(x => x != null).Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;

            _viewSegment.ValueChanged += (sender, args) => ViewModel.ShownIndex = _viewSegment.SelectedSegment;
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
            base.ViewWillDisappear(animated);
        }
    }
}

