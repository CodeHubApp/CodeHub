using System;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Images;
using MonoTouch.UIKit;
using CodeHub.iOS.Cells;
using System.Drawing;

namespace CodeHub.iOS.Elements
{
    public class GistElement : Element, IImageUpdated, IElementSizing
    {
        private readonly string _name;
        private readonly string _time;
        private readonly string _description;
        private string _imageUrl;
        private UIImage _image;
        private Action _tapped;

        public GistElement(string name, string description, string time, string imageUrl, Action tapped = null)
        {
            _tapped = tapped;
            _time = time;
            _name = name;
            _description = description;
            _imageUrl = imageUrl;
        }

        public override void Selected(UITableView tableView, MonoTouch.Foundation.NSIndexPath path)
        {
            if (_tapped != null)
                _tapped();
            base.Selected(tableView, path);
        }

        public void UpdatedImage(Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            if (img != null)
            {
                var cell = GetActiveCell() as GistCellView;
                if (cell != null)
                {
                    cell.Image = img;
                    cell.SetNeedsDisplay();
                }
            }
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(GistCellView.Key) as GistCellView ?? GistCellView.Create();

            if (!string.IsNullOrEmpty(_imageUrl))
            {
                Uri uri;
                cell.Image = Uri.TryCreate(_imageUrl, UriKind.Absolute, out uri) ? 
                    ImageLoader.DefaultRequestImage(uri, this) : null;
            }
            else
            {
                cell.Image = _image;
            }

            cell.Time = _time;
            cell.Content = _description;
            cell.Title = _name;
            return cell;
        }

        public float GetHeight(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (GetRootElement() == null)
                return 44f;

            var cell = GetRootElement().GetOffscreenCell(GistCellView.Key, () => GistCellView.Create());
            cell.Time = _time;
            cell.Content = _description;
            cell.Title = _name;

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            cell.Bounds = new RectangleF(0, 0, tableView.Bounds.Width, tableView.Bounds.Height);

            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            return cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height + 1;
        }
    }
}

