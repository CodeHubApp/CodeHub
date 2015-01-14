using System;
using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class DialogStringElement : StyledStringElement
    {
        public DialogStringElement(string title, Action action, UIImage img)
            : base(title, action, img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate))
        {
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = base.GetCell(tv);
            cell.ImageView.TintColor = Themes.Theme.Current.PrimaryNavigationBarColor;
            return cell;
        }
    }
}

