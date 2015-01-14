using MonoTouch.UIKit;

namespace Xamarin.Utilities.Images
{
    public static class BackgroundHelper
    {
        private static bool IsTall
        {
            get 
            { 
                return UIDevice.CurrentDevice.UserInterfaceIdiom 
                    == UIUserInterfaceIdiom.Phone 
                    && UIScreen.MainScreen.Bounds.Height 
                    * UIScreen.MainScreen.Scale >= 1136;
            }     
        }

        public static UIImage LoadSplashImage()
        {
            UIImage bgImage = null;
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                bgImage = UIImageHelper.FromFileAuto(IsTall ? "Default-568h" : "Default");
            }
            else
            {
                if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                    bgImage = UIImageHelper.FromFileAuto("Default-Portrait");
                else
                    bgImage = UIImageHelper.FromFileAuto("Default-Landscape");
            }
            return bgImage;
        }

        public static UIColor CreateRepeatingBackground()
        {
            UIImage bgImage = LoadSplashImage();
            if (bgImage == null)
                return null;

            var size = new System.Drawing.SizeF(40f, bgImage.Size.Height);
            UIGraphics.BeginImageContext(size);
            var ctx = UIGraphics.GetCurrentContext();
            ctx.TranslateCTM (0, bgImage.Size.Height);
            ctx.ScaleCTM (1f, -1f);

            ctx.DrawImage(new System.Drawing.RectangleF(-10, 0, bgImage.Size.Width, bgImage.Size.Height), bgImage.CGImage);
            ctx.ClipToRect(new System.Drawing.RectangleF(0, 0, size.Width, size.Height));

            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            bgImage.Dispose();

            if (img == null)
                return null;

            var ret = UIColor.FromPatternImage(img);
            img.Dispose();
            return ret;
        }
    }
}

