using System;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MonoTouch.UIKit
{
    public static class UIImageHelper
    {
        /// <summary>
        /// Load's an image via the FromFile.
        /// Also checks to make sure it's on the main thread.
        /// </summary>
        /// <returns>The file auto.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="extension">Extension.</param>
        public static UIImage FromFileAuto(string filename, string extension = "png")
        {
            UIImage img = null;
            if (Foundation.NSThread.Current.IsMainThread)
                img = LoadImageFromFile(filename, extension);
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    img = LoadImageFromFile(filename, extension);
                });
            }

            return img;
        }

        private static UIImage LoadImageFromFile(string filename, string extension = "png")
        {
            if (UIScreen.MainScreen.Scale > 1.0)
            {
                var file = filename + "@2x." + extension;
                return System.IO.File.Exists(file) ? UIImage.FromFile(file) : UIImage.FromFile(filename + "." + extension);
            }
            else
            {
                var file = filename + "." + extension;
                return System.IO.File.Exists(file) ? UIImage.FromFile(file) : UIImage.FromFile(filename + "@2x." + extension);
            }
        }

        public static UIImage ConvertToGrayScale (UIImage image)
        {
            var imageRect = new RectangleF (PointF.Empty, image.Size);
            using (var colorSpace = CGColorSpace.CreateDeviceGray ())
            using (var context = new CGBitmapContext (IntPtr.Zero, (int) imageRect.Width, (int) imageRect.Height, 8, 0, colorSpace, CGImageAlphaInfo.None)) {
                context.DrawImage (imageRect, image.CGImage);
                using (var imageRef = context.ToImage ())
                    return new UIImage (imageRef);
            }
        }
    }
}

