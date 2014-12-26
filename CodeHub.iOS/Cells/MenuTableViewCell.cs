using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class MenuTableViewCell : ReactiveTableViewCell
    {
        public static NSString Key = new NSString("menucell");
        private const float ImageSize = 24f;
        private readonly UILabel _numberView;

        public int NotificationNumber { get; set; }

        public bool RoundedImage { get; set; }

        public MenuTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            BackgroundColor = UIColor.Clear;
            TextLabel.TextColor = Themes.Theme.Current.MenuTextColor;
            SelectedBackgroundView = new UIView { BackgroundColor = Themes.Theme.Current.MenuSelectedBackgroundColor };

            _numberView = new UILabel { BackgroundColor = Themes.Theme.Current.PrimaryNavigationBarColor };
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
                ImageView.Frame = new RectangleF(0, 0, ImageSize, ImageSize);
                ImageView.Center = new PointF(ImageSize, center.Y);

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
                _numberView.Frame = new RectangleF(ContentView.Bounds.Width - 44, 11, 34, 22f);
                _numberView.Text = NotificationNumber.ToString();
                AddSubview(_numberView);
            }
            else
            {
                _numberView.RemoveFromSuperview();
            }
        }
    }
}

