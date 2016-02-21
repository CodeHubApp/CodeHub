using Foundation;
using UIKit;
using CoreGraphics;

namespace System
{
    public static class StringExtensions
    {
        public static nfloat MonoStringHeight(this string s, UIFont font, nfloat maxWidth)
        {
            if (string.IsNullOrEmpty(s))
                return 0f;
            
            using (var str = new NSString (s))
            {
                return str.StringSize(font, new CGSize(maxWidth, 1000), UILineBreakMode.WordWrap).Height;
            }
        }
    }
}

