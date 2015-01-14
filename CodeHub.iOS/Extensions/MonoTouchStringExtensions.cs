using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace System
{
	public static class MonoTouchStringExtensions
	{
		public static float MonoStringLength(this string s, UIFont font)
		{
			if (string.IsNullOrEmpty(s))
				return 0f;
				
			using (var str = new NSString (s))
			{
				return str.StringSize(font).Width;
			}
		}
		
		public static float MonoStringHeight(this string s, UIFont font, float maxWidth)
		{
			if (string.IsNullOrEmpty(s))
				return 0f;
			
			using (var str = new NSString (s))
			{
				return str.StringSize(font, new SizeF(maxWidth, 1000), UILineBreakMode.WordWrap).Height;
			}
		}
	}
}

