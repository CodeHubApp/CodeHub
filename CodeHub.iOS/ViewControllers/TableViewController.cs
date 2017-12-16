using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace CodeHub.iOS.ViewControllers
{
    public class TableViewController : BaseViewController
    {
        private readonly Lazy<UITableView> _tableView;
        private UIRefreshControl _refreshControl;

        public UITableView TableView => _tableView.Value;

        public bool ClearSelectionOnAppear { get; set; } = true;

        public virtual UIRefreshControl RefreshControl
        {
            get { return _refreshControl; }
            set
            {
                _refreshControl?.RemoveFromSuperview();
                _refreshControl = value;

                if (_refreshControl != null)
                    TableView.AddSubview(_refreshControl);
            }
        }

        public TableViewController(UITableViewStyle style)
        {
            _tableView = new Lazy<UITableView>(() => new UITableView(CGRect.Empty, style));

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Frame = View.Bounds;
            TableView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
            TableView.AutosizesSubviews = true;
            TableView.CellLayoutMarginsFollowReadableWidth = false;
            TableView.EstimatedSectionFooterHeight = 0;
            TableView.EstimatedSectionHeaderHeight = 0;
            Add(TableView);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHideNotification);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);

            var index = TableView.IndexPathForSelectedRow;
            if (ClearSelectionOnAppear && index != null)
                TableView.DeselectRow(index, true);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            View.EndEditing(true);
            
            if (_hideNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        private void OnKeyboardHideNotification(NSNotification notification)
        {
            TableView.ContentInset = UIEdgeInsets.Zero;
            TableView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }

        private void OnKeyboardNotification (NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
            var inset = new UIEdgeInsets(0, 0, keyboardFrame.Height, 0);
            TableView.ContentInset = inset;
            TableView.ScrollIndicatorInsets = inset;
        }
    }
}

