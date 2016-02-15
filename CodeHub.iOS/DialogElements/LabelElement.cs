using CoreGraphics;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class LabelElement : StringElement
    {
        public string Name { get; private set; }

        public LabelElement(string name, string color)
            : base(name)
        {
            Name = name;
            Image = CreateImage(color);
        }

        private static UIImage CreateImage(string color)
        {
            try
            {
                var red = color.Substring(0, 2);
                var green = color.Substring(2, 2);
                var blue = color.Substring(4, 2);

                var redB = System.Convert.ToByte(red, 16);
                var greenB = System.Convert.ToByte(green, 16);
                var blueB = System.Convert.ToByte(blue, 16);

                var size = new CGSize(28f, 28f);
                var cgColor = UIColor.FromRGB(redB, greenB, blueB).CGColor;

                UIGraphics.BeginImageContextWithOptions(size, false, 0);
                var ctx = UIGraphics.GetCurrentContext();
                ctx.SetLineWidth(1.0f);
                ctx.SetStrokeColor(cgColor);
                ctx.AddEllipseInRect(new CGRect(0, 0, size.Width, size.Height));
                ctx.SetFillColor(cgColor);
                ctx.FillPath();

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }

}

