using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class InputElement : EntryElement
    {
        public InputElement(string caption, string placeholder, string value)
            : base(caption, placeholder, value)
        {
            TitleFont = StyledStringElement.DefaultTitleFont.WithSize(StyledStringElement.DefaultTitleFont.PointSize);
            EntryFont = UIFont.SystemFontOfSize(14);
            TitleColor = StyledStringElement.DefaultTitleColor;
        }
    }
}

