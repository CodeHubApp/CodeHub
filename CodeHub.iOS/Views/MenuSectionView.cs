using CoreGraphics;
using UIKit;

namespace CodeHub.iOS.Views
{
    public class MenuSectionView : UIView
    {
        public static UIColor DefaultBackgroundColor;
        public static UIColor DefaultTextColor;

        public MenuSectionView(string caption)
            : base(new CGRect(0, 0, 320, 27))
        {
            this.BackgroundColor = DefaultBackgroundColor;

            var label = new UILabel(); 
			label.BackgroundColor = UIColor.Clear;
            label.Opaque = false; 
            label.TextColor = DefaultTextColor;
            label.Font =  UIFont.BoldSystemFontOfSize(12.5f);
            label.Frame = new CGRect(8,1,200,23); 
            label.Text = caption; 
            this.AddSubview(label); 
        }
    }
}

