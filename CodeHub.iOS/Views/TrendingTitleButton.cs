using UIKit;

namespace CodeHub.iOS.Views
{
    public class TrendingTitleButton : UIButton
    {
        private readonly UILabel _label;
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

        public override UIColor TintColor
        {
            get
            {
                return base.TintColor;
            }
            set
            {
                _label.TextColor = value; 
                _imageView.TintColor = value;
                base.TintColor = value;
            }
        }

        public TrendingTitleButton()
        {
            _label = new UILabel();
            _label.TextAlignment = UITextAlignment.Center;
            Add(_label);

            _imageView = new UIImageView();
            _imageView.Frame = new CoreGraphics.CGRect(0, 0, 12, 12);
            _imageView.Image = Images.DownChevron.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            Add(_imageView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _label.SizeToFit();
            _label.Center = new CoreGraphics.CGPoint(Frame.Width / 2f, Frame.Height / 2f);
            _imageView.Center = new CoreGraphics.CGPoint(_label.Frame.Right + 12f, Frame.Height / 2f);
        }
    }
}

