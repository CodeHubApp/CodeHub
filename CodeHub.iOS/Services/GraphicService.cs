using System;
using CodeHub.Core.Services;
using System.Drawing;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Services
{
    public class GraphicService : IGraphicService
    {
        public object CreateLabelImage(Color color)
        {
            var size = new SizeF(28f, 28f);
            var cgColor = UIColor.FromRGB(color.R, color.G, color.B).CGColor;

            UIGraphics.BeginImageContextWithOptions(size, false, 0);
            var ctx = UIGraphics.GetCurrentContext();
            ctx.SetLineWidth(1.0f);
            ctx.SetStrokeColor(cgColor);
            ctx.AddEllipseInRect(new RectangleF(0, 0, size.Width, size.Height));
            ctx.SetFillColor(cgColor);
            ctx.FillPath();

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}

