using System;
using MonoTouch.Dialog.Utilities;
using UIKit;
using CoreGraphics;

namespace CodeFramework.Views
{
    public class ProfileButton : UIButton, IImageUpdated
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
                _imageView.Image = ImageLoader.DefaultRequestImage(value, this);
            }
        }

        public ProfileButton()
            : base(UIButtonType.Custom)
        {
            this.AutosizesSubviews = true;

            _imageView = new UIImageView(new CGRect(new CGPoint(0, 0), this.Frame.Size));
            _imageView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _imageView.Layer.MasksToBounds = true;
            _imageView.Layer.CornerRadius = 4.0f;

//            this.Layer.ShadowColor = UIColor.Black.CGColor;
//            this.Layer.ShadowOpacity = 0.3f;
//            this.Layer.ShadowOffset = new SizeF(0, 1);
//            this.Layer.ShadowRadius = 4.0f;

            this.AddSubview(_imageView);
        }

        public void UpdatedImage(Uri uri)
        {
            var img = ImageLoader.DefaultRequestImage(uri, this);
            if (img == null)
                return;

            _imageView.Image = img;
            _imageView.SetNeedsDisplay();
        }
    }
}

