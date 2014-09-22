using System;
using MonoTouch.UIKit;
using System.Drawing;
using SDWebImage;
using MonoTouch.Foundation;

namespace CodeHub.iOS.ViewComponents
{
    public class MenuProfileView : UIButton
    {
        private string _imageUrl;
        private readonly UIImageView _imageView;
        private readonly UILabel _nameLabel;
        private readonly UILabel _usernameLabel;

        public string ImageUri
        {
            get { return _imageUrl; }
            set 
            {
                _imageUrl = value;
                _imageView.SetImage(new NSUrl(_imageUrl), Images.LoginUserUnknown);
            }
        }

        public string Name
        {
            get { return _nameLabel.Text; }
            set { _nameLabel.Text = value; }
        }

        public string Username
        {
            get { return _usernameLabel.Text; }
            set { _usernameLabel.Text = value; }
        }

        public MenuProfileView(RectangleF rect)
            : base(rect)
        {
            _imageView = new UIImageView();
            _imageView.Layer.MasksToBounds = true;
            Add(_imageView);

            _nameLabel = new UILabel();
            _nameLabel.Font = UIFont.SystemFontOfSize(15f);
            _nameLabel.TextColor = UIColor.White;
            Add(_nameLabel);

            _usernameLabel = new UILabel();
            _usernameLabel.Font = UIFont.SystemFontOfSize(12f);
            _usernameLabel.TextColor = UIColor.LightGray;
            Add(_usernameLabel);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var imageSize = Bounds.Height - 8f;
            _imageView.Frame = new RectangleF(new PointF(0, 4f), new SizeF(imageSize, imageSize));
            _imageView.Layer.CornerRadius = _imageView.Frame.Height / 2f;

            if (Bounds.Height < 40f)
            {
                var labelX = _imageView.Frame.Right + 8f;
                _nameLabel.Frame = new RectangleF(labelX, 1f, Bounds.Width - labelX, Bounds.Height - 2f);
                _usernameLabel.Hidden = true;
                _usernameLabel.Frame = RectangleF.Empty;
            }
            else
            {
                var labelX = _imageView.Frame.Right + 8f;
                _nameLabel.Frame = new RectangleF(labelX, Bounds.Height / 2f - 16f, Bounds.Width - labelX, 16f);
                _usernameLabel.Hidden = false;
                _usernameLabel.Frame = new RectangleF(_nameLabel.Frame.X, _nameLabel.Frame.Bottom + 1f, _nameLabel.Frame.Width, 15f);
            }
        }
    }
}

