using System;
using System.Collections.Generic;
using System.Drawing;
using CodeHub.iOS.ViewComponents;

// Analysis disable once CheckNamespace
namespace MonoTouch.UIKit
{
    public static class UITextViewExtensions
    {
        public static void CreateAccessoryInputView(this UITextView @this, IEnumerable<UIButton> buttons)
        {
            var normalButtonImage = ImageFromColor(UIColor.White);
            var pressedButtonImage = ImageFromColor(UIColor.FromWhiteAlpha(0.0f, 0.4f));

            foreach (var button in buttons)
            {
                button.SetBackgroundImage(normalButtonImage, UIControlState.Normal);
                button.SetBackgroundImage(pressedButtonImage, UIControlState.Highlighted);
            }

            var height = CalculateHeight(UIApplication.SharedApplication.StatusBarOrientation);
            var s = new ScrollingToolbarView(new RectangleF(0, 0, @this.Bounds.Width, height), buttons);
            s.BackgroundColor = UIColor.FromRGB(212, 214, 219);
            @this.InputAccessoryView = s;
        }

        private static UIImage ImageFromColor(UIColor color)
        {
            UIGraphics.BeginImageContext(new SizeF(1, 1));
            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(new RectangleF(0, 0, 1, 1));
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        public static UIButton CreateAccessoryButton(this UITextView This, UIImage image, Action action)
        {
            var btn = CreateAccessoryButton(This, string.Empty, action);
            btn.SetImage(image, UIControlState.Normal);
            btn.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            btn.ImageEdgeInsets = new UIEdgeInsets(6, 6, 6, 6);
            return btn;
        }

        public static UIButton CreateAccessoryButton(this UITextView This, string title, Action action)
        {
            var fontSize = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 22 : 28f;
            var btn = new UIButton(UIButtonType.System);
            btn.Frame = new RectangleF(0, 0, 32, 32);
            btn.SetTitle(title, UIControlState.Normal);
            btn.BackgroundColor = UIColor.White;
            btn.Font = UIFont.SystemFontOfSize(fontSize);
            btn.Layer.CornerRadius = 7f;
            btn.Layer.MasksToBounds = true;
            btn.AdjustsImageWhenHighlighted = false;
            btn.TouchUpInside += (object sender, System.EventArgs e) => action();
            return btn;
        }

        private static float CalculateHeight(UIInterfaceOrientation orientation)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return 44;

            // If  pad
            if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
                return 64;
            return 88f;
        }
    }
}

