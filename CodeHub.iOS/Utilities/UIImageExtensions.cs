using System;
using CoreGraphics;
using UIKit;

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
                UIApplication.SharedApplication.InvokeOnMainThread(() => {
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
    }
}

