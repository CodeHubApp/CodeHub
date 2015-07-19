using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Activity;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using System.Linq;

namespace CodeHub.iOS.ViewControllers.Activity
{
    public class NotificationsViewController : BaseTableViewController<NotificationsViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Unread", "Participating", "All" });
        private readonly UIBarButtonItem _segmentBarButton;
        private readonly UIBarButtonItem[] _segmentToolbar;
        private readonly UIBarButtonItem[] _markToolbar;
        private readonly UIBarButtonItem _markButton;
        private readonly UIBarButtonItem _editButton;
        private readonly UIBarButtonItem _cancelButton;

        public NotificationsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Inbox.ToEmptyListImage(), "No new notifications."));

            _markButton = new UIBarButtonItem(string.Empty, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.ReadSelectedCommand.ExecuteIfCan());

            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            _markToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _markButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            ToolbarItems = _segmentToolbar;

            _editButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit, (s, e) => StartEditing());
            _cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (s, e) => StopEditing());

            var groupedNotificationsObservable = this.WhenAnyValue(x => x.ViewModel.GroupedNotifications).IsNotNull();

            this.WhenActivated(d => {
                
                d(groupedNotificationsObservable
                    .SelectMany(x => x)
                    .SelectMany(x => x.Notifications)
                    .Select(x => x.WhenAnyValue(y => y.IsSelected))
                    .Merge()
                    .Select(_ => ViewModel.GroupedNotifications.SelectMany(x => x.Notifications).Any(x => x.IsSelected))
                    .Where(x => TableView.Editing)
                    .Subscribe(x =>
                    {
                        _markButton.Title = x ? "Mark Selected as Read" : "Read All as Read";
                    }));

                d(groupedNotificationsObservable
                    .Where(x => x.Count == 0 && TableView.Editing)
                    .Subscribe(_ => StopEditing()));

                d(groupedNotificationsObservable
                    .Subscribe(x => _editButton.Enabled = x.Count > 0));

                d(this.WhenAnyValue(x => x.ViewModel.ActiveFilter)
                    .Subscribe(x => 
                    {
                        _viewSegment.SelectedSegment = x;
                        NavigationItem.SetRightBarButtonItem((_viewSegment.SelectedSegment != 2) ? _editButton : null, true);
                    }));
            });

            Appearing
                .Where(_ => NavigationController != null)
                .Subscribe(x => NavigationController.SetToolbarHidden(false, x));

            Disappearing
                .Where(_ => NavigationController != null)
                .Subscribe(x => NavigationController.SetToolbarHidden(true, x));
        }

        private void StartEditing()
        {
            var allSelected = ViewModel.GroupedNotifications.SelectMany(x => x.Notifications).Any(x => x.IsSelected);
            _markButton.Title = allSelected ? "Mark Selected as Read" : "Read All as Read";

            NavigationItem.SetRightBarButtonItem(_cancelButton, true);
            TableView.SetEditing(true, true);
            SetToolbarItems(_markToolbar, true);
        }

        private void StopEditing()
        {
            NavigationItem.SetRightBarButtonItem(_editButton, true);
            TableView.SetEditing(false, true);
            SetToolbarItems(_segmentToolbar, true);

            foreach (var n in ViewModel.GroupedNotifications.SelectMany(x => x.Notifications))
                n.IsSelected = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var notificationSource = new NotificationTableViewSource(TableView);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            ViewModel.WhenAnyValue(x => x.GroupedNotifications).Where(x => x != null).Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;

            _viewSegment.ValueChanged += (sender, args) => ViewModel.ActiveFilter = (int)_viewSegment.SelectedSegment;
        }
    }
}

