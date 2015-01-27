using System;
using UIKit;
using CodeHub.iOS.Utilities;

// Analysis disable once CheckNamespace
namespace CodeHub
{
    public static class OcticonExtensions
    {
        public static UIImage ToImage(this Octicon @this, nfloat size)
        {
            return Graphics.ImageFromFont(UIFont.FromName("octicons", size), @this.CharacterCode, UIColor.Black)
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}

