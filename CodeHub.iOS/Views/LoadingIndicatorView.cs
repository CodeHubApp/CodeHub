using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views
{
    public sealed class LoadingIndicatorView : UIActivityIndicatorView
    {
        public static UIColor DefaultColor;

        public LoadingIndicatorView(bool active = true)
            : base(UIActivityIndicatorViewStyle.White)
        {
            if (active)
                StartAnimating();

            Frame = new CGRect(0, 0, 320f, 88f);
            Color = DefaultColor;
        }
    }
}

