using System;
using UIKit;

namespace CodeFramework.iOS.ViewComponents
{
    public class ToolbarButton
    {
        public static UIButton Create(UIImage image, Action action = null)
        {
            var button = new UIButton(UIButtonType.Custom) {Frame = new CoreGraphics.CGRect(0, 0, 40f, 40f)};
            button.SetImage(image, UIControlState.Normal);
            if (action != null)
                button.TouchUpInside += (s, e) => action();
            return button;
        }
    }
}

