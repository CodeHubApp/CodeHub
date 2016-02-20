using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class NoItemsElement : StringElement
    {
        public NoItemsElement()
            : this(null)
        {
        }

        public NoItemsElement(string text)
            : base(text ?? "No Items")
        {
            Accessory = UITableViewCellAccessory.None;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var c = base.GetCell(tv);
            c.TextLabel.TextAlignment = UITextAlignment.Center;
            return c;
        }
    }
}

