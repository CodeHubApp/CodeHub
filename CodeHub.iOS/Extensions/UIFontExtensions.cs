// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIFontExtensions
    {
        public static UIFont MakeBold(this UIFont font)
        {
            return UIFont.FromDescriptor(font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), font.PointSize);
        }

        public static UIFont MakeItalic(this UIFont font)
        {
            return UIFont.FromDescriptor(font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Italic), font.PointSize);
        }
    }
}

