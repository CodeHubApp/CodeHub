using UIKit;
using CoreGraphics;
using System;

namespace CodeHub.iOS.Views
{
    public class SlideUpTitleView : UIView
    {
        private readonly UILabel _label;

        public string Text
        {
            get { return _label.Text; }
            set 
            { 
                _label.Text = value;
                SetNeedsLayout();

                if (Superview != null)
                    Superview.SetNeedsLayout();
            }
        }

        public nfloat Offset
        {
            get { return _label.Frame.Y; }
            set
            {
                if (value < 0)
                    value = 0;
                var f = _label.Frame;
                f.Y = value;
                _label.Frame = f;
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var h = _label.SizeThatFits(size);
            return new CGSize(h.Width, base.SizeThatFits(size).Height);
        }

        public SlideUpTitleView(float height)
            : base(new CGRect(0, 0, 10, height))
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

            _label = new UILabel(Bounds);
            _label.Font = UIFont.SystemFontOfSize(18f);
            _label.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            _label.TextAlignment = UITextAlignment.Center;
            _label.LineBreakMode = UILineBreakMode.TailTruncation;
            _label.TextColor = UIColor.White;
            Add(_label);

            Layer.MasksToBounds = true;
        }
    }
}