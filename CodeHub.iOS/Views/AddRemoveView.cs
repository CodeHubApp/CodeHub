using System;
using UIKit;
using Foundation;
using CodeHub.iOS;

namespace CodeHub.iOS.Views
{
    public class AddRemoveView : UIView
    {
        public int? Added { get; set; }

        public int? Removed { get; set; }

        public AddRemoveView()
        {
            BackgroundColor = UIColor.Clear;
        }

        public AddRemoveView(IntPtr ptr)
            : base(ptr)
        {
            BackgroundColor = UIColor.Clear;
        }

        public override void Draw(CoreGraphics.CGRect rect)
        {
            base.Draw(rect);

            var width = rect.Width / 2f - 1f;
            var addedRect = new CoreGraphics.CGRect(0, 0, width, rect.Height);
            var removedRect = new CoreGraphics.CGRect(rect.Width / 2f + 1f, 0, width, rect.Height);

            var context = UIGraphics.GetCurrentContext();
            context.SaveState();
            context.SetFillColor(UIColor.FromRGB(204, 255, 204).CGColor);
            context.AddPath(Graphics.MakeRoundedRectPath(addedRect, 5));
            context.FillPath();

            context.SetFillColor(UIColor.FromRGB(255, 221, 221).CGColor);
            context.AddPath(Graphics.MakeRoundedRectPath(removedRect, 5));
            context.FillPath();

            context.RestoreState();

            UIColor.FromRGB(57, 152, 57).SetColor();
            var stringRect = addedRect;
            stringRect.Y += 1f;
            string addedString = (Added == null) ? "-" : "+" + Added.Value;
            new NSString(addedString).DrawString(stringRect, UIFont.SystemFontOfSize(12f), UILineBreakMode.TailTruncation, UITextAlignment.Center);

            UIColor.FromRGB(0xcc, 0x33, 0x33).SetColor();
            stringRect = removedRect;
            stringRect.Y += 1f;
            string removedString = (Removed == null) ? "-" : "-" + Removed.Value;
            new NSString(removedString).DrawString(stringRect, UIFont.SystemFontOfSize(12f), UILineBreakMode.TailTruncation, UITextAlignment.Center);
        }
    }
}

