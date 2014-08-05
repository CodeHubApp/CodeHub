using System;
using CodeFramework.iOS.Cells;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Images;
using System.Drawing;

namespace CodeFramework.iOS.Elements
{
    public class RepositoryElement : Element, IElementSizing, IImageUpdated
    {       
        private readonly string _name;
        private readonly int _followers;
        private readonly int _forks;
        private readonly string _description;
        private readonly string _owner;
        private UIImage _image;
        private readonly Uri _imageUri;

        public UIColor BackgroundColor { get; set; }

        public bool ShowOwner { get; set; }

        public event NSAction Tapped;

        public RepositoryElement(string name, int followers, int forks, string description, string owner, Uri imageUri = null, UIImage image = null)
        {
            _name = name;
            _followers = followers;
            _forks = forks;
            _description = description;
            _owner = owner;
            _imageUri = imageUri;
            _image = image;
            ShowOwner = true;
        }
        
        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(RepositoryCellView.Key) as RepositoryCellView ?? RepositoryCellView.Create();
            if (_image == null && _imageUri != null)
                _image = ImageLoader.DefaultRequestImage(_imageUri, this);
            cell.Bind(_name, _followers.ToString(), _forks.ToString(), _description, ShowOwner ? _owner : null, _image);
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            if (Tapped != null)
                Tapped();
        }

        public void UpdatedImage(Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            var activeCell = GetActiveCell() as RepositoryCellView;
            if (activeCell != null && img != null)
                activeCell.RepositoryImage = img;
        }

        public float GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            if (GetRootElement() == null)
                return 44f;

            var cell = GetRootElement().GetOffscreenCell(RepositoryCellView.Key, () => RepositoryCellView.Create());
            cell.Bind(_name, _followers.ToString(), _forks.ToString(), _description, ShowOwner ? _owner : null, _image);

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            cell.Bounds = new RectangleF(0, 0, tableView.Bounds.Width, tableView.Bounds.Height);

            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            return cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height + 1;
        }
    }
}

