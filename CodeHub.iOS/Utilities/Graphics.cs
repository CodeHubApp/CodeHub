using CoreGraphics;
using UIKit;
using Foundation;
using System.Drawing;
using System;

namespace CodeHub.iOS.Utilities
{
    public static class Graphics
    {
        public static UIImage ImageFromFont(UIFont font, char character, UIColor fillColor)
        {
            var s = new NSString("" + character);
            var stringSize = s.StringSize(font);
            var maxSize = (nfloat)Math.Max(stringSize.Height, stringSize.Width);
            var size = new CGSize(maxSize, maxSize);

            UIGraphics.BeginImageContextWithOptions(size, false, 0f);
            fillColor.SetFill();

            var drawPoint = new CGPoint((size.Width / 2f) - (stringSize.Width / 2f),
                                (size.Height / 2f) - (stringSize.Height / 2f));
            s.DrawString(drawPoint, font);

            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return img;
        }

        public static UIImage CreateLabelImage(Color color)
        {
            var size = new CGSize(28f, 28f);
            var cgColor = UIColor.FromRGB(color.R, color.G, color.B).CGColor;

            UIGraphics.BeginImageContextWithOptions(size, false, 0);
            var ctx = UIGraphics.GetCurrentContext();
            ctx.SetLineWidth(1.0f);
            ctx.SetStrokeColor(cgColor);
            ctx.AddEllipseInRect(new CGRect(0, 0, size.Width, size.Height));
            ctx.SetFillColor(cgColor);
            ctx.FillPath();

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}

