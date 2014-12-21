using MonoTouch.UIKit;
using Xamarin.Utilities.DialogElements;
using System;
using CodeHub.iOS.Cells;
using SDWebImage;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Elements
{
    public class MenuElement : Element
    {
        private readonly UIImage _staticImage;
        private readonly string _imageUri;
        private readonly Action _tapped;
        private readonly string _title;

        public int NotificationNumber { get; set; }

        public MenuElement(string title, Action tapped, UIImage image = null, string imageUri = null)
        {
            _title = title;
            _tapped = tapped;
            _staticImage = image;
            _imageUri = imageUri;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            if (_tapped != null)
                _tapped();
            base.Selected(tableView, path);
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(MenuTableViewCell.Key) as MenuTableViewCell ?? new MenuTableViewCell();
            cell.NotificationNumber = NotificationNumber;
            cell.TextLabel.Text = _title;
            if (string.IsNullOrEmpty(_imageUri))
                cell.ImageView.Image = _staticImage;
            else
                cell.ImageView.SetImage(new NSUrl(_imageUri), _staticImage);
            return cell;
        }
    }

}