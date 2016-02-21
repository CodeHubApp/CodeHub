namespace CodeHub.iOS.DialogElements
{
    public class InputElement : EntryElement
    {
        public InputElement(string caption, string placeholder, string value)
            : base(caption, placeholder, value)
        {
            TitleFont = StringElement.DefaultTitleFont;
            TitleColor = StringElement.DefaultTitleColor;
        }
    }
}

