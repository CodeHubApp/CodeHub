using System;
using UIKit;
using CoreGraphics;
using SDWebImage;
using Foundation;

namespace CodeHub.iOS.ViewComponents
{
    public class MenuProfileView : UIButton
    {
        private string _imageUrl;
        private readonly UIImageView _imageView;
        private readonly UILabel _titleLabel;
        private readonly UILabel _subtitleLabel;

        public string ImageUri
        {
            get { return _imageUrl; }
            set 
            {
                _imageUrl = value;
                _imageView.SetImage(new NSUrl(_imageUrl), Images.LoginUserUnknown);
            }
        }

        public string Title
        {
            get { return _titleLabel.Text; }
            set { _titleLabel.Text = value; }
        }

        public string Subtitle
        {
            get { return _subtitleLabel.Text; }
            set 
            { 
                _subtitleLabel.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    SetNeedsLayout();
                    LayoutIfNeeded();
                }
            }
        }

        public MenuProfileView(CGRect rect)
            : base(rect)
        {
            _imageView = new UIImageView();
            _imageView.Layer.MasksToBounds = true;
            Add(_imageView);

            _titleLabel = new UILabel();
            _titleLabel.Font = UIFont.SystemFontOfSize(15f);
            _titleLabel.TextColor = UIColor.White;
            Add(_titleLabel);

            _subtitleLabel = new UILabel();
            _subtitleLabel.Font = UIFont.SystemFontOfSize(12f);
            _subtitleLabel.TextColor = UIColor.LightGray;
            Add(_subtitleLabel);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var imageSize = Bounds.Height - 8f;
            _imageView.Frame = new CGRect(new CGPoint(0, 4f), new CGSize(imageSize, imageSize));
            _imageView.Layer.CornerRadius = _imageView.Frame.Height / 2f;

            var labelX = _imageView.Frame.Right + 8f;

            if (Bounds.Height < 40f)
            {
                _titleLabel.Frame = new CGRect(labelX, 1f, Bounds.Width - labelX, Bounds.Height - 2f);
                _subtitleLabel.Hidden = true;
                _subtitleLabel.Frame = CGRect.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(Subtitle))
                {
                    _titleLabel.Frame = new CGRect(labelX, 0, Bounds.Width - labelX, Bounds.Height);
                    _subtitleLabel.Hidden = true;
                    _subtitleLabel.Frame = CGRect.Empty;
                }
                else
                {
                    _titleLabel.Frame = new CGRect(labelX, Bounds.Height / 2f - 16f, Bounds.Width - labelX, 16f);
                    _subtitleLabel.Hidden = false;
                    _subtitleLabel.Frame = new CGRect(_titleLabel.Frame.X, _titleLabel.Frame.Bottom + 1f, _titleLabel.Frame.Width, 15f);
                }
            }
        }
    }
}

