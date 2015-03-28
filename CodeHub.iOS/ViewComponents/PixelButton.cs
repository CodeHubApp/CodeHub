using System;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.ViewComponents
{
    public class PixelButton : UIButton
    {
        public readonly UILabel Label;

        public PixelButton(UIColor color = null)
            : base(UIButtonType.RoundedRect)
        {
            Label = new UILabel();
            Label.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            Label.TextAlignment = UITextAlignment.Center;
            Add(Label);

            this.Layer.MasksToBounds = true;
            this.Layer.CornerRadius = 6f;
            this.Layer.BorderWidth = 1f;

            if (color != null)
            {
                this.Layer.BorderColor = color.CGColor;
                Label.TextColor = color;
            }

            var originalColor = BackgroundColor;
            TouchDown += (sender, e) => BackgroundColor = color;
            TouchUpOutside += (sender, e) => BackgroundColor = originalColor;
            TouchUpInside += (sender, e) => BackgroundColor = originalColor;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            Label.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);
        }
    }
}

