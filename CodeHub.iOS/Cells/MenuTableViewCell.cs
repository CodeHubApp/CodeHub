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
        private const float ImageSize = 16f;
        private readonly UILabel _numberView;

        public int NotificationNumber { get; set; }

        public MenuTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            BackgroundColor = UIColor.Clear;
            TextLabel.TextColor = UIColor.FromRGB(213, 213, 213);
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
                ImageView.Frame = new RectangleF(0, 0, ImageSize, ImageSize);
                ImageView.Center = new PointF(ImageSize, center.Y);

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

