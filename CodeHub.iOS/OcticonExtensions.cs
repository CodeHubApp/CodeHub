using System;
using UIKit;
using CodeHub.iOS.Utilities;
using System.IO;
using Foundation;

// Analysis disable once CheckNamespace
namespace CodeHub
{
    public static class OcticonExtensions
    {
        private static readonly bool IsRetnia;

        static OcticonExtensions()
        {
            IsRetnia = UIScreen.MainScreen.Scale > 1;
        }

        public static UIImage ToImage(this Octicon @this, nfloat size, bool cache = true)
        {
            var cacheDir = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User)[0].Path;
            var fileName = string.Format("octicon-{0}-{1}{2}.png", (int)@this.CharacterCode, size, IsRetnia ? "@2x" : string.Empty);
            var combinedPath = Path.Combine(cacheDir, fileName);

            if (File.Exists(combinedPath))
            {
                var img = cache ? UIImage.FromBundle(combinedPath) : UIImage.FromFile(combinedPath);
                return img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
            else
            {
                var img = Graphics.ImageFromFont(UIFont.FromName("octicons", size), @this.CharacterCode, UIColor.Black);
                var pngData = img.AsPNG();
                pngData.Save(combinedPath, false);
                return img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
        }

        public static UIImage ToImage(this Octicon @this, bool cache = true)
        {
            return @this.ToImage(24f, cache);
        }

        public static UIImage ToEmptyListImage(this Octicon @this)
        {
            return @this.ToImage(64f, false);
        }
    }
}

