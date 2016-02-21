using System;
using CoreGraphics;
using UIKit;
using Foundation;

namespace CodeHub.iOS
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
            nfloat minx = rect.Left;
            nfloat midx = rect.Left + (rect.Width)/2;
            nfloat maxx = rect.Right;
            nfloat miny = rect.Top;
            nfloat midy = rect.Y+rect.Size.Height/2;
            nfloat maxy = rect.Bottom;

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
                var p = Graphics.MakeRoundedRectPath (rect, radius);
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

