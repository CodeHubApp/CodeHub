using System;
using UIKit;
using CoreGraphics;
using Foundation;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class MenuTableViewCell : ReactiveTableViewCell
    {
        public static NSString Key = new NSString("menucell");
        private const float ImageSize = 24f;
        private readonly UILabel _numberView = new UILabel();

        private int _notificationNumber;
        public int NotificationNumber 
        {
            get { return _notificationNumber; }
            set
            {
                _notificationNumber = value;
                _numberView.Text = value.ToString();
            }
        }

        public bool RoundedImage { get; set; }

        public MenuTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            BackgroundColor = UIColor.Clear;
            TextLabel.TextColor = Themes.Theme.Current.MenuTextColor;
            SelectedBackgroundView = new UIView { BackgroundColor = Themes.Theme.Current.MenuSelectedBackgroundColor };

            _numberView.BackgroundColor = Themes.Theme.Current.PrimaryNavigationBarColor;
            _numberView.Layer.MasksToBounds = true;
            _numberView.Layer.CornerRadius = 5f;
            _numberView.TextAlignment = UITextAlignment.Center;
            _numberView.TextColor = UIColor.White;
            _numberView.Font = UIFont.SystemFontOfSize(14f);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (ImageView != null)
            {
                var center = ImageView.Center;
                ImageView.Frame = new CGRect(0, 0, ImageSize, ImageSize);
                ImageView.Center = new CGPoint(ImageSize, center.Y);

                if (RoundedImage)
                {
                    ImageView.Layer.MasksToBounds = true;
                    ImageView.Layer.CornerRadius = ImageSize / 2f;
                }
                else
                {
                    ImageView.Layer.MasksToBounds = false;
                    ImageView.Layer.CornerRadius = 0;
                }

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
                _numberView.Frame = new CGRect(0, 0, 38, 28f);
                _numberView.Center = new CGPoint(ContentView.Bounds.Width - 44, ContentView.Bounds.Height / 2f);
                _numberView.Text = NotificationNumber.ToString();
                AddSubview(_numberView);
            }
            else
            {
                _numberView.RemoveFromSuperview();
            }

            SeparatorInset = new UIEdgeInsets(0, TextLabel.Frame.Left, 0, 0);
        }
    }
}

