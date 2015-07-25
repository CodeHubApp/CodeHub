using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views
{
    public class LoadingIndicatorView : UIActivityIndicatorView
    {
        public LoadingIndicatorView()
            : base(UIActivityIndicatorViewStyle.White)
        {
            Frame = new CGRect(0, 0, 320f, 88f);
            Color = Theme.PrimaryNavigationBarColor;
        }
    }
}

