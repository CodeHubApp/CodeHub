using System;
using CodeFramework.iOS.Cells;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Images;

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

        public float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            var descriptionHeight = 0f;
            if (!string.IsNullOrEmpty(_description))
                descriptionHeight = _description.MonoStringHeight(RepositoryCellView.DescriptionFont, tableView.Bounds.Width - 56f - 28f) + 8f;
            return 52f + descriptionHeight;
        }
        
        public event NSAction Tapped;
        
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
    }
}

