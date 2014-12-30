using MonoTouch.UIKit;

namespace CodeHub.iOS
{
    public class Theme
    {
        public static UIColor PrimaryNavigationBarColor
        {
            get { return UIColor.FromRGB(50, 50, 50); }
        }

        public static UIColor PrimaryNavigationBarTextColor
        {
            get { return UIColor.White; }
        }

        public static UIColor MainTextColor 
        { 
            get { return UIColor.FromRGB(41, 41, 41); } 
        }

        public static UIColor MainTitleColor 
        { 
            get { return UIColor.FromRGB(0x41, 0x83, 0xc4); } 
        }

        public static UIColor MainSubtitleColor 
        { 
            get { return UIColor.FromRGB(81, 81, 81); } 
        }
    }
}
