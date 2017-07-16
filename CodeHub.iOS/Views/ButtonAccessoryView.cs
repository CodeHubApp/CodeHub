using System;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;

namespace CodeHub.iOS.Views
{
    public class ButtonAccessoryView : UIView
    {
        private readonly ScrollingToolbarView _scrollingToolBar;

        public ButtonAccessoryView(IEnumerable<UIButton> buttons)
        {
            Frame = new CGRect(0, 0, 320f, 44f);

            var normalButtonImage = ImageFromColor(UIColor.White);
            var pressedButtonImage = ImageFromColor(UIColor.FromWhiteAlpha(0.0f, 0.4f));

            foreach (var button in buttons)
            {
                button.SetBackgroundImage(normalButtonImage, UIControlState.Normal);
                button.SetBackgroundImage(pressedButtonImage, UIControlState.Highlighted);
            }

            _scrollingToolBar = new ScrollingToolbarView(new CGRect(0, 0, Bounds.Width, Bounds.Height), buttons);
            _scrollingToolBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _scrollingToolBar.BackgroundColor = UIColor.FromWhiteAlpha(0.84f, 1.0f);
            Add(_scrollingToolBar);
        }

        public static UIButton CreateAccessoryButton(UIImage image, Action action)
        {
            var btn = CreateAccessoryButton(string.Empty, action);
            btn.SetImage(image, UIControlState.Normal);
            btn.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            btn.ImageEdgeInsets = new UIEdgeInsets(6, 6, 6, 6);
            return btn;
        }

        public static UIButton CreateAccessoryButton(string title, Action action)
        {
            var fontSize = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 22 : 28f;
            var btn = new UIButton(UIButtonType.System);
            btn.Frame = new CGRect(0, 0, 26, 32);
            btn.SetTitle(title, UIControlState.Normal);
            btn.BackgroundColor = UIColor.White;
            btn.Font = UIFont.BoldSystemFontOfSize(fontSize);
            btn.Layer.CornerRadius = 7f;
            btn.Layer.MasksToBounds = true;
            btn.AdjustsImageWhenHighlighted = false;
            btn.TouchUpInside += (sender, e) => action();
            return btn;
        }


        private static UIImage ImageFromColor(UIColor color)
        {
            UIGraphics.BeginImageContext(new CGSize(1, 1));
            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(new CGRect(0, 0, 1, 1));
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}
