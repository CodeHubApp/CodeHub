using CoreGraphics;
using UIKit;
using System;
using CodeHub.iOS.Views;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class MenuBaseViewController : ViewModelDrivenDialogViewController
    {
        readonly ProfileButton _profileButton;
        readonly UILabel _title;

        protected MenuBaseViewController()
            : base(false, UITableViewStyle.Plain)
        {
            _title = new UILabel(new CGRect(0, 40, 320, 40));
            _title.TextAlignment = UITextAlignment.Left;
            _title.BackgroundColor = UIColor.Clear;
            _title.Font = UIFont.SystemFontOfSize(16f);
            _title.TextColor = UIColor.FromRGB(246, 246, 246);
//            _title.ShadowColor = UIColor.FromRGB(21, 21, 21);
//            _title.ShadowOffset = new SizeF(0, 1);
            NavigationItem.TitleView = _title;

            _profileButton = new ProfileButton();

            OnActivation(d =>
            {
                d(_profileButton.GetClickedObservable().Subscribe(_ => ProfileButtonClicked()));
            });
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public override string Title {
            get {
                return _title == null ? base.Title : " " + _title.Text;
            }
            set {
                if (_title != null)
                    _title.Text = " " + value;
                base.Title = value;
            }
        }

        public ProfileButton ProfileButton
        {
            get { return _profileButton; }
        }

        /// <summary>
        /// Invoked when it comes time to set the root so the child classes can create their own menus
        /// </summary>
        protected abstract void CreateMenuRoot();

        private void UpdateProfilePicture()
        {
            var size = new CGSize(32, 32);
            if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft ||
                UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                size = new CGSize(24, 24);
            }

            _profileButton.Frame = new CGRect(new CGPoint(0, 4), size);

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(_profileButton);
        }

        protected virtual void ProfileButtonClicked ()
        {
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add some nice looking colors and effects
            TableView.SeparatorColor = UIColor.FromRGB(14, 14, 14);
            TableView.TableFooterView = new UIView(new CGRect(0, 0, View.Bounds.Width, 0));
            TableView.BackgroundColor = UIColor.FromRGB(34, 34, 34);

            //Prevent the scroll to top on this view
            this.TableView.ScrollsToTop = false;
        }
        
        public override void ViewWillAppear(bool animated)
        {
            UpdateProfilePicture();
            CreateMenuRoot();
            base.ViewWillAppear(animated);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            UpdateProfilePicture();
        }

        protected class MenuElement : StringElement
        {
            private int _notificationNumber;
            public int NotificationNumber
            {
                get { return _notificationNumber; }
                set
                {
                    if (value == _notificationNumber)
                        return;
                    _notificationNumber = value;
                    var cell = GetActiveCell() as Cell;
                    if (cell != null)
                        cell.NotificationNumber = value;
                }
            }

            public MenuElement(string title, Action tapped, UIImage image, Uri imageUrl = null) : base(title)
            {
                Clicked.Subscribe(_ => tapped?.Invoke());
                TextColor = UIColor.FromRGB(213, 213, 213);
                Image = image;
                ImageUri = imageUrl;
                Accessory = UITableViewCellAccessory.None;
            }

            //We want everything to be the same size as far as images go.
            //So, during layout, we'll resize the imageview and pin it to a specific size!
            private class Cell : UITableViewCell
            {
                private const float ImageSize = 20f;
                private UILabel _numberView;

                private int _notificationNumber;
                public int NotificationNumber 
                {
                    get { return _notificationNumber; }
                    set
                    {
                        _notificationNumber = value;
                        SetNeedsLayout();
                        LayoutIfNeeded();
                    }
                }

                public Cell(UITableViewCellStyle style, string key)
                    : base(style, key)
                {
                    SelectedBackgroundView = new UIView { BackgroundColor = UIColor.FromRGB(25, 25, 25) };

                    _numberView = new UILabel { BackgroundColor = UIColor.FromRGB(54, 54, 54) };
                    _numberView.Layer.MasksToBounds = true;
                    _numberView.Layer.CornerRadius = 5f;
                    _numberView.TextAlignment = UITextAlignment.Center;
                    _numberView.TextColor = UIColor.White;
                    _numberView.Font = UIFont.SystemFontOfSize(12f);
                }

                public override void LayoutSubviews()
                {
                    base.LayoutSubviews();
                    if (ImageView != null)
                    {
                        var center = ImageView.Center;
                        ImageView.Frame = new CGRect(0, 0, ImageSize, ImageSize);
                        ImageView.Center = new CGPoint(ImageSize, center.Y);

                        if (TextLabel != null)
                        {
                            var frame = TextLabel.Frame;
                            frame.X = ImageSize * 2;
                            frame.Width += (TextLabel.Frame.X - frame.X);
                            TextLabel.Frame = frame;
                        }
                    }

                    if (NotificationNumber > 0)
                    {
                        _numberView.Frame = new CGRect(ContentView.Bounds.Width - 44, 11, 34, 22f);
                        _numberView.Text = NotificationNumber.ToString();
                        AddSubview(_numberView);
                    }
                    else
                    {
                        _numberView.RemoveFromSuperview();
                    }
                }
            }

            public override UITableViewCell GetCell(UITableView tv)
            {
                var cell = base.GetCell(tv) as Cell;
                cell.NotificationNumber = NotificationNumber;
                cell.ImageView.Layer.CornerRadius = ImageUri != null ? (cell.ImageView.Frame.Height / 2) : 0;
                cell.BackgroundColor = UIColor.Clear;
                return cell;
            }

            protected override UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
            {
                var cell = new Cell(style, key);
                cell.ImageView.Layer.MasksToBounds = true;
                cell.ImageView.TintColor = UIColor.FromRGB(0xd5, 0xd5, 0xd5);
                return cell;
            }
        }
    }
}

