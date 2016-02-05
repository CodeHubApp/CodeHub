using Foundation;
using UIKit;
using CoreGraphics;

namespace System
{
	public static class StringExtensions
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

        public static string ToOneLine(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace("\n", " ").Replace("\r","");
        }

        public static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
	}
}

