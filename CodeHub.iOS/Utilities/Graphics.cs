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

//        public static UIImage ImageFromFontWithBackground(UIFont font, char character, UIColor fillColor, UIColor backgroundColor)
//        {
//            var fontSize = font.PointSize;
//            font = font.WithSize(fontSize - 6f);
//            var s = new NSString("" + character);
//            var stringSize = s.StringSize(font);
//            //var maxSize = (nfloat)Math.Max(stringSize.Height, stringSize.Width);
//            var size = new CGSize(fontSize, fontSize);
//
//            UIGraphics.BeginImageContextWithOptions(size, false, 0f);
//            backgroundColor.SetFill();
//
//            FillRoundedRect(UIGraphics.GetCurrentContext(), new CGRect(0, 0, fontSize, fontSize), 4f);
//
//            fillColor.SetFill();
//
//            var drawPoint = new CGPoint((size.Width / 2f) - (stringSize.Width / 2f),
//                (size.Height / 2f) - (stringSize.Height / 2f));
//            s.DrawString(drawPoint, font);
//
//            var img = UIGraphics.GetImageFromCurrentImageContext();
//            UIGraphics.EndImageContext();
//            return img;
//        }
//
//        public static void FillRoundedRect (CGContext ctx, CGRect rect, nfloat radius)
//        {
//            var p = MakeRoundedRectPath (rect, radius);
//            ctx.AddPath (p);
//            ctx.FillPath ();
//        }
//
//        public static CGPath MakeRoundedPath (float size, float radius)
//        {
//            float hsize = size/2;
//
//            var path = new CGPath ();
//            path.MoveToPoint (size, hsize);
//            path.AddArcToPoint (size, size, hsize, size, radius);
//            path.AddArcToPoint (0, size, 0, hsize, radius);
//            path.AddArcToPoint (0, 0, hsize, 0, radius);
//            path.AddArcToPoint (size, 0, size, hsize, radius);
//            path.CloseSubpath ();
//
//            return path;
//        }
//
//        public static CGPath MakeRoundedRectPath (CGRect rect, nfloat radius)
//        {
//            nfloat minx = rect.Left;
//            nfloat midx = rect.Left + (rect.Width)/2;
//            nfloat maxx = rect.Right;
//            nfloat miny = rect.Top;
//            nfloat midy = rect.Y+rect.Size.Height/2;
//            nfloat maxy = rect.Bottom;
//
//            var path = new CGPath ();
//            path.MoveToPoint (minx, midy);
//            path.AddArcToPoint (minx, miny, midx, miny, radius);
//            path.AddArcToPoint (maxx, miny, maxx, midy, radius);
//            path.AddArcToPoint (maxx, maxy, midx, maxy, radius);
//            path.AddArcToPoint (minx, maxy, minx, midy, radius);        
//            path.CloseSubpath ();
//
//            return path;
//        }
//

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

