using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.MapKit;
using System;

namespace CodeHub.iOS.ViewComponents
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

        public float Offset
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

        public override SizeF SizeThatFits(SizeF size)
        {
            var h = _label.SizeThatFits(size);
            return new SizeF(h.Width, base.SizeThatFits(size).Height);
        }

        public SlideUpTitleView(float height)
            : base(new RectangleF(0, 0, 10, height))
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

