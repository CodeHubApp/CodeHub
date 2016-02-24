using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class PaginateElement : LoadMoreElement
    {
        static PaginateElement()
        {
            Padding = 20;
        }

        public PaginateElement(string normal, string loading)
        {
            NormalCaption = normal;
            LoadingCaption = loading;
            Font = UIFont.PreferredBody;
            TextColor = StringElement.DefaultTitleColor;
        }

        protected override void CellCreated(UITableViewCell cell, UIView view)
        {
            base.CellCreated(cell, view);
            cell.BackgroundColor = UIColor.White;
        }
    }
}

