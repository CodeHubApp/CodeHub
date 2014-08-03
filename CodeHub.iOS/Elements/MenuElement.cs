using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;
using System;

namespace CodeFramework.iOS.Elements
{
    public class MenuElement : StyledStringElement
    {
        public int NotificationNumber { get; set; }

        public MenuElement(string title, Action tapped, UIImage image)
            : base(title, tapped)
        {
            BackgroundColor = UIColor.Clear;
            TextColor = UIColor.FromRGB(213, 213, 213);
            DetailColor = UIColor.White;
            Image = image;
        }

        //We want everything to be the same size as far as images go.
        //So, during layout, we'll resize the imageview and pin it to a specific size!
        private class Cell : UITableViewCell
        {
            private const float ImageSize = 16f;
            private readonly UILabel _numberView;

            public int NotificationNumber { get; set; }

            public Cell(UITableViewCellStyle style, string key)
                : base(style, key)
            {
                //                    var v = new UIView(new RectangleF(0, 0, Frame.Width, 1)) { 
                //                        BackgroundColor = UIColor.FromRGB(44, 44, 44)
                //                    };
                //
                //                    AddSubview(v);
                //                    TextLabel.ShadowColor = UIColor.Black;
                //                    TextLabel.ShadowOffset = new SizeF(0, -1); 
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

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = base.GetCell(tv) as Cell;
            cell.NotificationNumber = NotificationNumber;
            return cell;
        }

        protected override UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            return new Cell(style, key);
        }
    }

}