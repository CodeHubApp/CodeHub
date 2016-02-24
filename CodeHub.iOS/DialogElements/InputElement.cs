using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class InputElement : EntryElement
    {
        public InputElement(string caption, string placeholder, string value)
            : base(caption, placeholder, value)
        {
            TitleFont = UIFont.PreferredBody;
            TitleColor = StringElement.DefaultTitleColor;
        }
    }
}

