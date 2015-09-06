using Foundation;
using System;

namespace CodeFramework.iOS.Views
{
    [Register("UILabelWithLinks")]
    public class UILabelWithLinks : MonoTouch.TTTAttributedLabel.TTTAttributedLabel
    {
        public UILabelWithLinks()
        {
        }

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

