using UIKit;
using CoreGraphics;
using System;

namespace CodeHub.iOS.Views
{
    public class ProgressBarView : UIView
    {
        private readonly ProgressBarIndicator _indicator;
        private readonly UILabel _label;

        private int _percentage;

        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;
                _label.Text = value + "%";
                SetNeedsLayout();
            }
        }

        public ProgressBarView()
            : base(new CGRect(0, 0, 320, 20))
        {
            AutosizesSubviews = true;

            _indicator = new ProgressBarIndicator();
            _indicator.Frame = new CGRect(0, 0, 0, Frame.Height);
            _indicator.BackgroundColor = UIColor.FromRGB(0x65, 0xBD, 0x10);
            Add(_indicator);

            _label = new UILabel();
            _label.Frame = new CGRect(10f, 1f, Frame.Width - 20f, Frame.Height - 2f);
            _label.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _label.BackgroundColor = UIColor.Clear;
            _label.UserInteractionEnabled = false;
            _label.Font = UIFont.BoldSystemFontOfSize(15f);
            _label.TextColor = UIColor.White;
            Add(_label);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _indicator.Frame = new CGRect(0, 0, Bounds.Width * (Percentage / 100f), Bounds.Height);
        }

        private class ProgressBarIndicator : UIView
        {
            public ProgressBarIndicator()
            {
                //UserInteractionEnabled = false;
                BackgroundColor = UIColor.FromRGB(0x65, 0xBD, 0x10);
            }

            public override void Draw(CGRect rect)
            {
                base.Draw(rect);

                var context = UIGraphics.GetCurrentContext();
                var colorSpace = CGColorSpace.CreateDeviceRGB();
                nfloat[] locations = { 0, 1 };

                CGColor[] colors =
                {
                    UIColor.FromRGB(0x8D, 0xCF, 0x16).CGColor,
                    UIColor.FromRGB(0x65, 0xBD, 0x10).CGColor,
                };

                var gradiend = new CGGradient(colorSpace, colors, locations);
                context.DrawLinearGradient(gradiend, new CGPoint(0, 0), new CGPoint(0, rect.Size.Height), CGGradientDrawingOptions.DrawsBeforeStartLocation);
                gradiend.Dispose();
                colorSpace.Dispose();
            }
        }

    }
}

