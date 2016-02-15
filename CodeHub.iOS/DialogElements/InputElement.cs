using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class InputElement : EntryElement
    {
        public InputElement(string caption, string placeholder, string value)
            : base(caption, placeholder, value)
        {
            TitleFont = StringElement.DefaultTitleFont;
            EntryFont = UIFont.SystemFontOfSize(14);
            TitleColor = StringElement.DefaultTitleColor;
        }
    }
}

