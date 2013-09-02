using System;
using MonoTouch.UIKit;

namespace CodeFramework.Images
{
    public static class Buttons
    {
        public static UIImage Back { get { return UIImageHelper.FromFileAuto("Images/Buttons/back"); } }
        public static UIImage ThreeLines { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
        public static UIImage Cancel { get { return UIImageHelper.FromFileAuto("Images/Buttons/cancel"); } }
        public static UIImage Edit { get { return UIImageHelper.FromFileAuto("Images/Buttons/edit"); } }
        public static UIImage Save { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        public static UIImage Add { get { return UIImageHelper.FromFileAuto("Images/Buttons/add"); } }
        public static UIImage Filter { get { return UIImageHelper.FromFileAuto("Images/Buttons/filter"); } }
        public static UIImage Gear { get { return UIImageHelper.FromFileAuto("Images/Buttons/gear"); } }
        public static UIImage View { get { return UIImageHelper.FromFileAuto("Images/Buttons/view"); } }
    }

    public static class Components
    {
        public static UIImage MenuSectionBackground { get { return UIImageHelper.FromFileAuto("Images/Components/menu_section_bg"); } }
        public static UIImage MenuNavbar { get { return UIImageHelper.FromFileAuto("Images/Components/menu_navbar"); } }
        public static UIImage Warning { get { return UIImageHelper.FromFileAuto("Images/Components/warning"); } }
    }

    public static class Views
    {
        public static UIImage Background;
    }

    public static class Web
    {
        public static UIImage Back { get { return UIImage.FromFile("Images/Web/back_button@2x.png"); } }
        public static UIImage Forward { get { return UIImage.FromFile("Images/Web/forward_button@2x.png"); } }
    }

	public static class Misc
	{
		public static UIImage Anonymous { get { return UIImageHelper.FromFileAuto("Images/Misc/anonymous"); } }
	}
}

