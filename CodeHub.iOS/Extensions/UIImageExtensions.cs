using System;
using System.Drawing;
using MonoTouch.CoreGraphics;

// Analysis disable once CheckNamespace
namespace MonoTouch.UIKit
{
    public static class UIImageExtensions
    {
        public static UIImage ConvertToGrayScale(this UIImage @this)
        {
            var imageRect = new RectangleF(PointF.Empty, @this.Size);
            using (var colorSpace = CGColorSpace.CreateDeviceGray())
            using (var context = new CGBitmapContext(IntPtr.Zero, (int)imageRect.Width, (int)imageRect.Height, 8, 0, colorSpace, CGImageAlphaInfo.None))
            {
                context.DrawImage(imageRect, @this.CGImage);
                using (var imageRef = context.ToImage())
                    return new UIImage(imageRef);
            }
        }
    }
}