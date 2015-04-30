using System;
using UIKit;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.DialogElements
{
    public class MultilinedElement : Element, IElementSizing
    {
        private string _caption;
        public new string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                if (_caption == value)
                    return;
                _caption = value;
                ReloadThis();
            }
        }

        private string _details;
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                if (_details == value)
                    return;
                _details = value;
                ReloadThis();
            }
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(MultilinedCellView.Key) as MultilinedCellView ?? MultilinedCellView.Create();
            cell.Caption = Caption;
            cell.Details = Details;

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            return cell;
        }

        public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var root = GetRootElement();
            if (root == null)
                return 44f;
            var cell = root.GetOffscreenCell<MultilinedCellView>(MultilinedCellView.Key, MultilinedCellView.Create);
            cell.Caption = Caption;
            cell.Details = Details;

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            cell.Bounds = new CoreGraphics.CGRect(0, 0, tableView.Bounds.Width, cell.Bounds.Height);

            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            var height = cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
            height += 1.0f;
            return height;
        }
    }
}

