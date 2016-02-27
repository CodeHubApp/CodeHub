using System;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.DialogElements
{
    public class MenuElement : StringElement
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
            private readonly UILabel _numberView;

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

