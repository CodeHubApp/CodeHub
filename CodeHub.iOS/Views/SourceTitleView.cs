using UIKit;

namespace CodeHub.iOS.Views
{
    public sealed class SourceTitleView : UIButton
    {
        private readonly UILabel _label = new UILabel();
        private readonly UILabel _subLabel = new UILabel();
        private readonly UIImageView _imageView;

        public string Text
        {
            get { return _label.Text; }
            set
            {
                _label.Text = value;
                SetNeedsLayout();
            }
        }

        public string SubText
        {
            get { return _subLabel.Text; }
            set
            {
                _subLabel.Text = value;
                SetNeedsLayout();
            }
        }

        public override UIColor TintColor
        {
            get
            {
                return base.TintColor;
            }
            set
            {
                _label.TextColor = value;
                _subLabel.TextColor = value;
                _imageView.TintColor = value;
                base.TintColor = value;
            }
        }

        public SourceTitleView()
            : base(new CoreGraphics.CGRect(0, 0, 200, 44f))
        {
            AutosizesSubviews = true;

            _label.TextAlignment = UITextAlignment.Center;
            _label.LineBreakMode = UILineBreakMode.TailTruncation;
            _label.Lines = 1;
            Add(_label);

            _subLabel.TextAlignment = UITextAlignment.Center;
            _subLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            _subLabel.Lines = 1;
            Add(_subLabel);

            _imageView = new UIImageView();
            _imageView.Image = Images.DownChevron.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            _imageView.Frame = new CoreGraphics.CGRect(0, 0, 8f, 8f);
            Add(_imageView);

            TintColor = UINavigationBar.Appearance.TintColor;
            AutoresizingMask = UIViewAutoresizing.All;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var small = Frame.Height < 44f;
            var halfHeight = Frame.Height / 2 - 3f;

            _label.Font = UIFont.SystemFontOfSize(small ? 14f : 18f);
            _label.SizeToFit();
            var labelFrame = _label.Frame;
            labelFrame.Height = _label.Frame.Height > halfHeight ? halfHeight : _label.Frame.Height;
            _label.Frame = labelFrame;
            _label.Center = new CoreGraphics.CGPoint(Frame.Width / 2f, _label.Frame.Height / 2f + (small ? 2f : 3f));

            _subLabel.Font = UIFont.SystemFontOfSize(small ? 10f : 12f);
            _subLabel.SizeToFit();
            var subLabelFrame = _subLabel.Frame;
            subLabelFrame.Height = _subLabel.Frame.Height > halfHeight ? halfHeight : _subLabel.Frame.Height;
            subLabelFrame.Width = _subLabel.Frame.Width >= Frame.Width ? Frame.Width - 16f : subLabelFrame.Width;
            _subLabel.Frame = subLabelFrame;
            _subLabel.Center = new CoreGraphics.CGPoint(Frame.Width / 2f, _label.Frame.Bottom + (_subLabel.Frame.Height / 2) + 2f);

            _imageView.Center = new CoreGraphics.CGPoint(_subLabel.Frame.Right + 8f, _subLabel.Center.Y);
        }
    }
}

