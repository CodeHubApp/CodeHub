using System;
using Foundation;
using UIKit;
using CoreGraphics;

namespace System
{
	public static class MonoTouchStringExtensions
	{
        public static nfloat MonoStringLength(this string s, UIFont font)
		{
			if (string.IsNullOrEmpty(s))
				return 0f;
				
			using (var str = new NSString (s))
			{
				return str.StringSize(font).Width;
			}
		}
		
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

