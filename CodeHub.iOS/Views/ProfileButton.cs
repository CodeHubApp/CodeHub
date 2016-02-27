using System;
using UIKit;
using CoreGraphics;
using CodeHub.iOS;
using SDWebImage;
using Foundation;

namespace CodeHub.iOS.Views
{
    public class ProfileButton : UIButton
    {
        private readonly UIImageView _imageView;
        private Uri _uri;

        public Uri Uri
        {
            get {
                return _uri;
            }
            set {
                _uri = value;
                if (value == null)
                    _imageView.Image = Images.Avatar;
                else
                    _imageView.SetImage(new NSUrl(value.AbsoluteUri), Images.Avatar);
            }
        }

        public ProfileButton()
        {
            this.AutosizesSubviews = true;

            _imageView = new UIImageView(new CGRect(new CGPoint(0, 0), this.Frame.Size));
            _imageView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _imageView.Layer.MasksToBounds = true;
            _imageView.Layer.CornerRadius = 4.0f;

            Add(_imageView);
        }
    }
}

