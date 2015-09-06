using System;
using CodeFramework.iOS.Cells;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;
using Foundation;
using UIKit;

namespace CodeFramework.iOS.Elements
{
    
    public class RepositoryElement : Element, IElementSizing, IColorizeBackground, IImageUpdated
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
            : base(null)
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

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            nfloat descriptionHeight = 0f;
            if (!string.IsNullOrEmpty(_description))
                descriptionHeight = _description.MonoStringHeight(RepositoryCellView.DescriptionFont, tableView.Bounds.Width - 56f - 28f) + 8f;
            return 52f + descriptionHeight;
        }
        
        protected override NSString CellKey {
            get {
                return new NSString("RepositoryCellView");
            }
        }
        
        
        public event Action Tapped;
        
        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CellKey) as RepositoryCellView ?? RepositoryCellView.Create();
            return cell;
        }
        
        public override bool Matches(string text)
        {
            return _name.ToLower().Contains(text.ToLower());
        }
        
        public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
        {
            base.Selected(dvc, tableView, path);
            if (Tapped != null)
                Tapped();
            tableView.DeselectRow (path, true);
        }
        
        void IColorizeBackground.WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            var c = cell as RepositoryCellView;
            if (c == null)
                return;

            if (_image == null && _imageUri != null)
                _image = ImageLoader.DefaultRequestImage(_imageUri, this);
            c.Bind(_name, _followers.ToString(), _forks.ToString(), _description, ShowOwner ? _owner : null, _image);
        }

        #region IImageUpdated implementation

        public void UpdatedImage(Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            if (img == null)
            {
                Console.WriteLine("DefaultRequestImage returned a null image");
                return;
            }
            _image = img;

            if (uri == null)
                return;
            var root = GetImmediateRootElement ();
            if (root == null || root.TableView == null)
                return;
            root.TableView.ReloadRows (new [] { IndexPath }, UITableViewRowAnimation.None);
        }

        #endregion
    }
}

