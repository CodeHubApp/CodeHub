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
        private readonly UIBarButtonItem[] _segmentToolbar;
        private readonly UIBarButtonItem[] _markToolbar;
        private readonly UIBarButtonItem _markButton;
        private readonly UIBarButtonItem _editButton;
        private readonly UIBarButtonItem _cancelButton;

        public NotificationsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Inbox.ToEmptyListImage(), "No new notifications."));

            _markButton = new UIBarButtonItem();
            _markToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _markButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            _editButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            _cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            {
                NavigationItem.TitleView = _viewSegment;
                _segmentToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            }
            else
            {
                _segmentToolbar = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), new UIBarButtonItem(_viewSegment), new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
                ToolbarItems = _segmentToolbar;
            }

            OnActivation(d => {
                d(_editButton.GetClickedObservable().Subscribe(_ => StartEditing()));
                d(_cancelButton.GetClickedObservable().Subscribe(_ => StopEditing()));
                d(_markButton.GetClickedObservable().InvokeCommand(ViewModel.ReadSelectedCommand));
                d(_viewSegment.GetChangedObservable().Subscribe(x => ViewModel.ActiveFilter = x));

                d(this.WhenAnyValue(x => x.ViewModel.ShowEditButton)
                    .Subscribe(x => NavigationItem.SetRightBarButtonItem(x ? _editButton : null, true)));

                d(this.WhenAnyValue(x => x.ViewModel.ActiveFilter)
                    .Subscribe(x => _viewSegment.SelectedSegment = x));

                d(this.WhenAnyValue(x => x.ViewModel.IsAnyItemsSelected)
                    .Subscribe(x => _markButton.Title = x ? "Mark Selected as Read" : "Mark All as Read"));

                d(this.WhenAnyObservable(x => x.ViewModel.Notifications.ItemChanged)
                    .Select(_ => ViewModel.Notifications.Count)
                    .Subscribe(x => {
                        _editButton.Enabled = x > 0;
                        if (x == 0 && TableView.Editing)
                            StopEditing();
                    }));
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null && !NavigationController.ToolbarHidden)
                NavigationController.SetToolbarHidden(true, animated);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && NavigationController != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        private void StartEditing()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                NavigationController.SetToolbarHidden(false, true);
            
            var allSelected = ViewModel.GroupedNotifications.SelectMany(x => x.Notifications).Any(x => x.IsSelected);
            _markButton.Title = allSelected ? "Mark Selected as Read" : "Read All as Read";

            NavigationItem.SetRightBarButtonItem(_cancelButton, true);
            TableView.SetEditing(true, true);
            SetToolbarItems(_markToolbar, true);
        }

        private void StopEditing()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                NavigationController.SetToolbarHidden(true, true);

            NavigationItem.SetRightBarButtonItem(_editButton, true);
            TableView.SetEditing(false, true);
            SetToolbarItems(_segmentToolbar, true);

            foreach (var n in ViewModel.GroupedNotifications.SelectMany(x => x.Notifications))
                n.IsSelected = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var viewModel = ViewModel;
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.Source = new NotificationTableViewSource(TableView, viewModel.GroupedNotifications, () => viewModel.ActiveFilter != NotificationsViewModel.AllFilter);
        }
    }
}

