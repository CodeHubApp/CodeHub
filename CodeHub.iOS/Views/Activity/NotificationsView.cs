using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Activity;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Activity
{
    public class NotificationsView : BaseTableViewController<NotificationsViewModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;
        private readonly UIBarButtonItem[] _segmentToolbar;
        private readonly UIBarButtonItem[] _markToolbar;
        private readonly UIBarButtonItem _markButton;
        private readonly UIBarButtonItem _editButton;
        private readonly UIBarButtonItem _cancelButton;


        public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread", "Participating", "All" });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);

            _markButton = new UIBarButtonItem(string.Empty, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.ReadSelectedCommand.ExecuteIfCan());

            _segmentToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            _markToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _markButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            ToolbarItems = _segmentToolbar;

            _editButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit, (s, e) =>
            {
                var allSelected = ViewModel.GroupedNotifications.SelectMany(x => x.Notifications).Any(x => x.IsSelected);
                _markButton.Title = allSelected ? "Mark Selected as Read" : "Read All as Read";

                NavigationItem.SetRightBarButtonItem(_cancelButton, true);
                TableView.SetEditing(true, true);
                SetToolbarItems(_markToolbar, true);
            });

            _cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (s, e) =>
            {
                NavigationItem.SetRightBarButtonItem(_editButton, true);
                TableView.SetEditing(false, true);
                SetToolbarItems(_segmentToolbar, true);

                foreach (var n in ViewModel.GroupedNotifications.SelectMany(x => x.Notifications))
                    n.IsSelected = false;
            });

            this.WhenActivated(d =>
            {
                d(this.WhenAnyValue(x => x.ViewModel.GroupedNotifications)
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

                d(this.WhenAnyValue(x => x.ViewModel.ActiveFilter)
                    .Subscribe(x => 
                    {
                        _viewSegment.SelectedSegment = x;
                        NavigationItem.SetRightBarButtonItem((_viewSegment.SelectedSegment != 2) ? _editButton : null, true);
                    }));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;

            var notificationSource = new NotificationTableViewSource(TableView);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            ViewModel.WhenAnyValue(x => x.GroupedNotifications).Where(x => x != null).Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;

            _viewSegment.ValueChanged += (sender, args) => ViewModel.ActiveFilter = (int)_viewSegment.SelectedSegment;
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

