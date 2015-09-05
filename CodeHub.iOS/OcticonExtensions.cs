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
                if (img == null)
                    return null;
                var pngData = img.AsPNG();
                pngData.Save(combinedPath, false);
                return img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
        }
//
//        private static UIColor[] colors = {
//            UIColor.FromRGB(26, 188, 156),
//            UIColor.FromRGB(46, 204, 113),
//            UIColor.FromRGB(52, 152, 219),
//            UIColor.FromRGB(155, 89, 182),
//            UIColor.FromRGB(52, 73, 94),
//            UIColor.FromRGB(22, 160, 133),
//            UIColor.FromRGB(39, 174, 96),
//            UIColor.FromRGB(41, 128, 185),
//            UIColor.FromRGB(142, 68, 173),
//            UIColor.FromRGB(44, 62, 80),
//            UIColor.FromRGB(241, 196, 15),
//            UIColor.FromRGB(230, 126, 34),
//            UIColor.FromRGB(231, 76, 60),
//            UIColor.FromRGB(149, 165, 166),
//            UIColor.FromRGB(243, 156, 18),
//            UIColor.FromRGB(211, 84, 0),
//            UIColor.FromRGB(192, 57, 43),
//            UIColor.FromRGB(127, 140, 141),
//        };
//
//        public static UIImage ToImageWithBackground(this Octicon @this)
//        {
//            UIColor bgColor = colors[(int)@this.CharacterCode % colors.Length];
//            return Graphics.ImageFromFontWithBackground(UIFont.FromName("octicons", 24f), @this.CharacterCode, UIColor.White, bgColor);
//        }
//
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

