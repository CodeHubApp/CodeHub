using System;
using UIKit;
using CoreGraphics;
using SDWebImage;
using Foundation;

namespace CodeHub.iOS.Views
{
    public class ImageAndTitleHeaderView : UIView
    {
        private readonly UIImageView _imageView;
        private readonly UILabel _label;
        private readonly UILabel _label2;
        private readonly UIView _seperatorView;
        private readonly UIView _subView;
        private readonly UIImageView _subImageView;

        private const float SubImageAppearTime = 0.25f;

        public UIButton ImageButton { get; private set; }

        public UIImageView SubImageView
        {
            get { return _subImageView; }
        }

        public Action ImageButtonAction { get; set; }

        public UIImage Image
        {
            get { return _imageView.Image; }
            set { _imageView.Image = value; }
        }

        public string Text
        {
            get { return _label.Text; }
            set 
            { 
                _label.Text = value; 
                this.SetNeedsLayout();
                this.LayoutIfNeeded();
            }
        }

        public UIColor TextColor
        {
            get { return _label.TextColor; }
            set
            {
                _label.TextColor = value;
            }
        }

        public string SubText
        {
            get { return _label2.Text; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _label2.Hidden = false;
                _label2.Text = value;
                this.SetNeedsLayout();
                this.LayoutIfNeeded();
            }
        }

        public UIColor SubTextColor
        {
            get { return _label2.TextColor; }
            set
            {
                _label2.TextColor = value;
            }
        }

        public bool EnableSeperator
        {
            get
            {
                return !_seperatorView.Hidden;
            }
            set
            {
                _seperatorView.Hidden = !value;
            }
        }

        public UIColor SeperatorColor
        {
            get
            {
                return _seperatorView.BackgroundColor;
            }
            set
            {
                _seperatorView.BackgroundColor = value;
            }
        }

        public bool RoundedImage
        {
            get { return _imageView.Layer.CornerRadius > 0; }
            set
            {
                if (value)
                {
                    _imageView.Layer.CornerRadius = _imageView.Frame.Width / 2f;
                    _imageView.Layer.MasksToBounds = true;
                }
                else
                {
                    _imageView.Layer.MasksToBounds = false;
                    _imageView.Layer.CornerRadius = 0;
                }
            }
        }

        public UIColor ImageTint
        {
            get { return _imageView.TintColor; }
            set { _imageView.TintColor = value; }
        }

        public ImageAndTitleHeaderView()
            : base(new CGRect(0, 0, 320f, 100f))
        {
            ImageButton = new UIButton(UIButtonType.Custom);
            ImageButton.Frame = new CGRect(0, 0, 80, 80);
            ImageButton.TouchUpInside += (sender, e) => {
                if (ImageButtonAction != null)
                    ImageButtonAction();
            };
            Add(ImageButton);

            _imageView = new UIImageView();
            _imageView.Frame = new CGRect(0, 0, 80, 80);
            _imageView.BackgroundColor = UIColor.White;
            _imageView.Layer.BorderWidth = 2f;
            _imageView.Layer.BorderColor = UIColor.White.CGColor;
            ImageButton.Add(_imageView);

            _label = new UILabel();
            _label.TextAlignment = UITextAlignment.Center;
            _label.Lines = 0;
            _label.Font = UIFont.PreferredHeadline;
            Add(_label);

            _label2 = new UILabel();
            _label2.Hidden = true;
            _label2.TextAlignment = UITextAlignment.Center;
            _label2.Font = UIFont.PreferredSubheadline;
            _label2.Lines = 0;
            Add(_label2);

            _seperatorView = new UIView();
            _seperatorView.BackgroundColor = UIColor.FromWhiteAlpha(214.0f / 255.0f, 1.0f);
            Add(_seperatorView);

            _subView = new UIView();
            _subView.Frame = new CGRect(56, 56, 22, 22);
            _subView.Layer.CornerRadius = 10f;
            _subView.Layer.MasksToBounds = true;
            _subView.BackgroundColor = UIColor.White;
            _subView.Hidden = true;
            ImageButton.Add(_subView);

            _subImageView = new UIImageView(new CGRect(0, 0, _subView.Frame.Width - 4f, _subView.Frame.Height - 4f));
            _subImageView.Center = new CGPoint(11f, 11f);
            _subView.Add(_subImageView);

            EnableSeperator = false;
            RoundedImage = true;
        }

        public void SetImage(Uri imageUri, UIImage placeholder)
        {
            if (imageUri == null)
                _imageView.Image = placeholder;
            else
            {
                _imageView.SetImage(new NSUrl(imageUri.AbsoluteUri), placeholder, (image, error, cacheType, imageUrl) => {
                    if (image != null && error == null)
                        UIView.Transition(_imageView, 0.35f, UIViewAnimationOptions.TransitionCrossDissolve, () => _imageView.Image = image, null);
                });
            }
        }

        public void SetSubImage(UIImage image)
        {
            if (image == null && _subImageView.Image != null)
            {
                UIView.Animate(SubImageAppearTime, 0, UIViewAnimationOptions.CurveEaseIn, () => 
                    _subView.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f), () =>
                {
                    _subView.Hidden = true;
                    _subImageView.Image = null;
                });
            }
            else if (image != null && _subImageView.Image != null)
            {
                UIView.Animate(SubImageAppearTime, 0, UIViewAnimationOptions.TransitionCrossDissolve, () => 
                    _subImageView.Image = image, null);
            }
            else if (image != null && _subImageView.Image == null)
            {
                _subView.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
                _subView.Hidden = false;
                _subImageView.Image = image;
                UIView.Animate(SubImageAppearTime, 0, UIViewAnimationOptions.CurveEaseIn, () => 
                    _subView.Transform = CGAffineTransform.MakeIdentity(), null);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ImageButton.Center = new CGPoint(Bounds.Width / 2, 15 + ImageButton.Frame.Height / 2);

            _label.Frame = new CGRect(20, ImageButton.Frame.Bottom + 10f, Bounds.Width - 40, Bounds.Height - (ImageButton.Frame.Bottom + 5f));
            _label.SizeToFit();
            _label.Frame = new CGRect(20, ImageButton.Frame.Bottom + 10f, Bounds.Width - 40, _label.Frame.Height);

            _label2.Frame = new CGRect(20, _label.Frame.Bottom + 2f, Bounds.Width - 40f, _label2.Font.LineHeight + 2f);
            _label2.SizeToFit();
            _label2.Frame = new CGRect(20, _label.Frame.Bottom + 2f, Bounds.Width - 40f, _label2.Frame.Height);

            var bottom = _label2.Hidden == false? _label2.Frame.Bottom : _label.Frame.Bottom;
            var f = Frame;
            f.Height = bottom + 15f;
            Frame = f;

            _seperatorView.Frame = new CGRect(0, Frame.Height - 0.5f, Frame.Width, 0.5f);
        }
    }
}

