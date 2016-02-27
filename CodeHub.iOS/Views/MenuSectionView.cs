using CoreGraphics;
using UIKit;

namespace CodeHub.iOS.Views
{
    public sealed class MenuSectionView : UIView
    {
        public MenuSectionView(string caption)
            : base(new CGRect(0, 0, 320, 27))
        {
            this.BackgroundColor = UIColor.FromRGB(50, 50, 50);

            var label = new UILabel(); 
            label.BackgroundColor = UIColor.Clear;
            label.Opaque = false; 
            label.TextColor = UIColor.FromRGB(171, 171, 171); 
            label.Font =  UIFont.BoldSystemFontOfSize(12.5f);
            label.Frame = new CGRect(8,1,200,23); 
            label.Text = caption; 
            this.Add(label); 
        }
    }
}

