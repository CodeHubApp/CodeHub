using Foundation;
using System;

namespace CodeHub.iOS.Views
{
    [Register("UILabelWithLinks")]
    public class UILabelWithLinks : Xamarin.TTTAttributedLabel.TTTAttributedLabel
    {
        public UILabelWithLinks(IntPtr ptr)
            : base(ptr)
        {
        }

        public UILabelWithLinks(NSCoder coder)
            : base(coder)
        {
        }
    }
}

