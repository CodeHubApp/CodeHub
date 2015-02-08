using CoreGraphics;
using UIKit;
using Foundation;
using System.Drawing;

namespace CodeHub.iOS.Utilities
{
    public static class Graphics
    {
        public static UIImage ImageFromFont(UIFont font, char character, UIColor fillColor)
        {
            var s = new NSString("" + character);
            var size = s.StringSize(font);
            UIGraphics.BeginImageContextWithOptions(size, false, 0f);
            fillColor.SetFill();
            s.DrawString(new CGPoint(0, 0), font);
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

        /// <summary>
        ///    Creates a path for a rectangle with rounded corners
        /// </summary>
        /// <param name="rect">
        /// The <see cref="RectangleF"/> rectangle bounds
        /// </param>
        /// <param name="radius">
        /// The <see cref="System.Single"/> size of the rounded corners
        /// </param>
        /// <returns>
        /// A <see cref="CGPath"/> that can be used to stroke the rounded rectangle
        /// </returns>
        public static CGPath MakeRoundedRectPath (CGRect rect, float radius)
        {
            var minx = rect.Left;
            var midx = rect.Left + (rect.Width)/2;
            var maxx = rect.Right;
            var miny = rect.Top;
            var midy = rect.Y+rect.Size.Height/2;
            var maxy = rect.Bottom;

            var path = new CGPath ();
            path.MoveToPoint (minx, midy);
            path.AddArcToPoint (minx, miny, midx, miny, radius);
            path.AddArcToPoint (maxx, miny, maxx, midy, radius);
            path.AddArcToPoint (maxx, maxy, midx, maxy, radius);
            path.AddArcToPoint (minx, maxy, minx, midy, radius);        
            path.CloseSubpath ();

            return path;
        }

        public static void FillRoundedRect (CGContext ctx, CGRect rect, float radius)
        {
            var p = MakeRoundedRectPath (rect, radius);
            ctx.AddPath (p);
            ctx.FillPath ();
        }

        public static CGPath MakeRoundedPath (float size, float radius)
        {
            float hsize = size/2;

            var path = new CGPath ();
            path.MoveToPoint (size, hsize);
            path.AddArcToPoint (size, size, hsize, size, radius);
            path.AddArcToPoint (0, size, 0, hsize, radius);
            path.AddArcToPoint (0, 0, hsize, 0, radius);
            path.AddArcToPoint (size, 0, size, hsize, radius);
            path.CloseSubpath ();

            return path;
        }
    }
}

