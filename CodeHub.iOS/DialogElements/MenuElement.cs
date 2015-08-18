using UIKit;
using System;
using CodeHub.iOS.Cells;
using SDWebImage;
using Foundation;

namespace CodeHub.iOS.DialogElements
{
    public class MenuElement : Element
    {
        private readonly UIImage _staticImage;
        private readonly string _imageUri;
        private readonly Action _tapped;
        private readonly string _title;


        private int _notificationNumber;
        public int NotificationNumber 
        {
            get { return _notificationNumber; }
            set
            {
                _notificationNumber = value;
                var cell = GetActiveCell() as MenuTableViewCell;
                if (cell != null) cell.NotificationNumber = value;
            }
        }

        public bool TintImage { get; set; }

        public MenuElement(string title, Action tapped, UIImage image, string imageUri = null)
        {
            if (title == null)
                throw new ArgumentNullException("title");
            if (tapped == null)
                throw new ArgumentNullException("tapped");
            if (image == null)
                throw new ArgumentNullException("image");

            _title = title;
            _tapped = tapped;
            _staticImage = image;
            _imageUri = imageUri;

            TintImage = true;
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

            if (TintImage && Theme.MenuIconColor != null)
                cell.ImageView.TintColor = Theme.MenuIconColor;
            else
                cell.ImageView.TintColor = null;

            cell.RoundedImage = false;

            if (string.IsNullOrEmpty(_imageUri))
            {
                if (TintImage)
                    cell.ImageView.Image = _staticImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                else
                    cell.ImageView.Image = _staticImage;
            }
            else
            {
                cell.RoundedImage = true;
                cell.ImageView.SetImage(new NSUrl(_imageUri), _staticImage, (image, error, cacheType, imageUrl) =>
                {
                    if (TintImage)
                        cell.ImageView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                });
            }
            return cell;
        }
    }

}