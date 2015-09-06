using Foundation;
using UIKit;
using System;

namespace MonoTouch.Dialog.Elements
{
    public class SpacingElement : Element, IElementSizing
    {
        private readonly float _height;

        public SpacingElement(float height) 
            : base(string.Empty)
        {
            _height = height;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = base.GetCell(tv);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.SeparatorInset = UIEdgeInsets.Zero;
            cell.BackgroundColor = UIColor.Clear;
            cell.ContentView.BackgroundColor = UIColor.Clear;
            return cell;
        }

        public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _height;
        }
    }
}