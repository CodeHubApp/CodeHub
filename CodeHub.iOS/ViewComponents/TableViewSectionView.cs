using CoreGraphics;
using UIKit;
using System;

namespace CodeHub.iOS.ViewComponents
{
    public class TableViewSectionView : UIView
    {
        readonly UILabel _lbl;

        public static CGGradient BottomGradient;
        public static CGGradient TopGradient;
        
        static TableViewSectionView ()
        {
            using (var rgb = CGColorSpace.CreateDeviceRGB()){
                var colorsBottom = new nfloat[] {
                    1, 1, 1, 0f,
                    0.4f, 0.4f, 0.4f, .6f
                };
                BottomGradient = new CoreGraphics.CGGradient (rgb, colorsBottom, null);
                var colorsTop = new nfloat[] {
                    0.8f, 0.8f, 0.8f, .4f,
                    1, 1, 1, 0f
                };
                TopGradient = new CoreGraphics.CGGradient (rgb, colorsTop, null);
            }
        }

        public TableViewSectionView(string text)
        {
            Frame = new CGRect(0, 0, 320, 24);
			//BackgroundColor = UIColor.FromPatternImage(Theme.CurrentTheme.TableViewSectionBackground).ColorWithAlpha(0.9f);
            
            _lbl = new UILabel();
            _lbl.Text = text;
            _lbl.TextColor = UIColor.FromRGB(140, 140, 140);
            _lbl.Font = UIFont.BoldSystemFontOfSize(14f);
            _lbl.BackgroundColor = UIColor.Clear;
            _lbl.ShadowColor = UIColor.FromRGBA(255, 255, 255, 225);
            _lbl.ShadowOffset = new CGSize(0, 1);
            _lbl.LineBreakMode = UILineBreakMode.TailTruncation;
            AddSubview(_lbl);
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _lbl.Frame = new CGRect(10, 2, Bounds.Width - 20f, Bounds.Height - 4f);
        }

        public override void Draw(CGRect rect)
        {
            var context = UIGraphics.GetCurrentContext();
            var bounds = Bounds;
            var midx = bounds.Width/2;
            
            base.Draw(rect);
            
            context.DrawLinearGradient (BottomGradient, new CGPoint (midx, bounds.Height-2), new CGPoint (midx, bounds.Height), 0);
            context.DrawLinearGradient (TopGradient, new CGPoint (midx, 0), new CGPoint (midx, 2), 0);
        }
    }
}

