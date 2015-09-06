using MonoTouch.Dialog;
using UIKit;

namespace MonoTouch.Dialog
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
            Font = StyledStringElement.DefaultTitleFont;
            TextColor = StyledStringElement.DefaultTitleColor;
        }

        protected override void CellCreated(UITableViewCell cell, UIView view)
        {
            base.CellCreated(cell, view);
            cell.BackgroundColor = UIColor.White;
        }
    }
}

