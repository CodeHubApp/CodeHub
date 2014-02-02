using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace CodeHub.iOS
{
    public static class Images
    {
        public static UIImage FromBundle(string path)
        {
            var img = UIImage.FromBundle(path);
            var template = img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return template;
        }

        public static UIImage FromFileAuto(string path)
        {
            var img = UIImageHelper.FromFileAuto(path);
            var template = img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return template;
        }

        public static UIImage WithTint(this UIImage img, UIColor color)
        {
            UIGraphics.BeginImageContextWithOptions(img.Size, false, img.CurrentScale);
            var context = UIGraphics.GetCurrentContext();
            context.TranslateCTM(0, img.Size.Height);
            context.ScaleCTM(1.0f, -1.0f);
            context.SetBlendMode(MonoTouch.CoreGraphics.CGBlendMode.Normal);
            var rect = new System.Drawing.RectangleF(0, 0, img.Size.Width, img.Size.Height);
            context.ClipToMask(rect, img.CGImage);
            color.SetFill();
            context.FillRect(rect);
            var i = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return i;
        }

        private static readonly Dictionary<string, UIImage> _retainedImages = new Dictionary<string, UIImage>();
        public static UIImage LoadTemplateImage(string path, UIColor color, bool retain = true)
        {
            if (_retainedImages.ContainsKey(path))
                return _retainedImages[path];

            using (var img = UIImageHelper.FromFileAuto(path))
            {
                using (var template = img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate))
                {
                    var tintedImage = template.WithTint(color);
                    if (retain)
                        _retainedImages.Add(path, tintedImage);
                    return tintedImage;
                }
            }
        }

        public static UIImage Merge { get { return FromBundle("/Images/merge"); } }
        public static UIImage Webpage { get { return FromBundle("/Images/webpage"); } }

        public static UIImage Pencil { get { return FromBundle("/Images/pencil"); } }
        public static UIImage Tag { get { return FromBundle("/Images/tag"); } }
        public static UIImage Milestone { get { return FromBundle("/Images/milestone"); } }
        public static UIImage Script { get { return FromBundle("/Images/script"); } }

        public static UIImage Create { get { return FromBundle("/Images/create"); } }

        public static UIImage File { get { return LoadTemplateImage("Images/file", UIColor.FromRGB(44, 62, 80)); } }

        public static UIImage Following { get { return LoadTemplateImage("Images/following", UIColor.FromRGB(192, 57, 43)); } }
        public static UIImage Folder { get { return LoadTemplateImage("Images/folder", UIColor.FromRGB(243, 156, 18)); } }

        public static UIImage BinClosed { get { return LoadTemplateImage("Images/bin_closed", UIColor.FromRGB(231, 76, 60)); } }

        public static UIImage Comments { get { return LoadTemplateImage("Images/comments", UIColor.FromRGB(142, 68, 173)); } }
        public static UIImage Heart { get { return LoadTemplateImage("Images/heart", UIColor.FromRGB(231, 76, 60)); } }

        public static UIImage Repo { get { return LoadTemplateImage("Images/repo", UIColor.FromRGB(52, 73, 94)); } }
        public static UIImage Branch { get { return LoadTemplateImage("Images/branch", UIColor.FromRGB(0x6c, 0xc6, 0x44)); } }

        public static UIImage Language { get { return LoadTemplateImage("Images/language", UIColor.FromRGB(52, 73, 94)); } }
        public static UIImage Size { get { return LoadTemplateImage("Images/size", UIColor.FromRGB(41, 128, 185)); } }

        public static UIImage Locked { get { return LoadTemplateImage("Images/locked", UIColor.FromRGB(241, 196, 15), false); } }
        public static UIImage Unlocked { get { return LoadTemplateImage("Images/unlocked", UIColor.FromRGB(241, 196, 15), false); } }

        public static UIImage Commit { get { return LoadTemplateImage("Images/commit", UIColor.FromRGB(0x15, 0x6f, 0x9e)); } }
        public static UIImage Fork { get { return LoadTemplateImage("Images/fork", UIColor.FromRGB(0x6c, 0xc6, 0x44)); } }
        public static UIImage Issue { get { return LoadTemplateImage("Images/flag", UIColor.FromRGB(0x6c, 0xc6, 0x44)); } }
        public static UIImage Hand { get { return LoadTemplateImage("Images/hand", UIColor.FromRGB(0x9e, 0x15, 0x7c)); } }

        public static UIImage Star { get { return LoadTemplateImage("Images/star",  UIColor.FromRGB(241, 196, 15)); } }

        public static UIImage User { get { return LoadTemplateImage("Images/user", UIColor.FromRGB(52, 152, 219)); } }
        public static UIImage Priority { get { return LoadTemplateImage("Images/priority", UIColor.FromRGB(243, 156, 18)); } }
        public static UIImage Cog { get { return LoadTemplateImage("Images/cog", UIColor.FromRGB(44, 62, 80)); } }

        public static UIImage Eye { get { return LoadTemplateImage("Images/eye", UIColor.FromRGB(22, 160, 133)); } }

        public static UIImage Event { get { return LoadTemplateImage("Images/events", UIColor.FromRGB(192, 57, 43)); } }

        public static UIImage Group { get { return LoadTemplateImage("Images/group", UIColor.FromRGB(39, 174, 96)); } }


        public static UIImage Anonymous { get { return FromBundle("/Images/anonymous"); } }



        public static UIImage Team { get { return FromFileAuto("Images/team"); } }

        //These only appear in the menu
        public static UIImage News { get { return FromFileAuto("Images/news"); } }
        public static UIImage Info { get { return FromFileAuto("Images/info"); } }
        public static UIImage Explore { get { return FromFileAuto("Images/explore"); } }
        public static UIImage Notifications { get { return FromFileAuto("Images/notifications"); } }
        public static UIImage Bug { get { return FromFileAuto("Images/bug"); } }


        public static UIImage MenuBackground { get { return UIImageHelper.FromFileAuto("Images/codehub-blur"); } }


        public static Uri GitHubRepoUrl
        {
            get { return new Uri(Path.Combine(NSBundle.MainBundle.ResourcePath, "Images/repository.png")); }
        }

        public static Uri GitHubRepoForkUrl
        {
            get { return new Uri(Path.Combine(NSBundle.MainBundle.ResourcePath, "Images/repository_fork.png")); }
        }
		
        public static class Logos
        {
            public static UIImage GitHub { get { return UIImage.FromFile("Images/Logos/github.png"); } }
        }

        public static class Buttons
        {
            public static UIImage BlackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/black_button"); } }
            public static UIImage GreyButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/grey_button"); } }
        }

        public static class Gist
        {
            public static UIImage Share { get { return UIImageHelper.FromFileAuto("Images/Gist/share"); } }
            public static UIImage Star { get { return UIImageHelper.FromFileAuto("Images/Gist/star"); } }
            public static UIImage StarHighlighted { get { return UIImageHelper.FromFileAuto("Images/Gist/star_highlighted"); } }
            public static UIImage User { get { return UIImageHelper.FromFileAuto("Images/Gist/user"); } }
        }

//        public static class Notifications
//        {
//            public static UIImage Commit { get { return UIImageHelper.FromFileAuto("Images/Notifications/commit"); } }
//            public static UIImage PullRequest { get { return UIImageHelper.FromFileAuto("Images/Notifications/pull_request"); } }
//        }
    }
}

