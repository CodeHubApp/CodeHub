using System;
using CoreGraphics;
using CodeFramework.iOS.Utils;
using CoreGraphics;
using MonoTouch.Dialog.Utilities;
using UIKit;
using Foundation;

namespace CodeFramework.iOS.Views
{
    public class HeaderView : UIView, IImageUpdated
    {
        private const float XPad = 14f;
        private const float YPad = 10f;

        public static UIFont TitleFont = UIFont.BoldSystemFontOfSize(16);
        public static UIFont SubtitleFont = UIFont.SystemFontOfSize(13);
        public static CGGradient Gradient;
        private string _title;
        private string _subtitle;
        private UIImage _image;
        private readonly UIFont _titleFont, _subtitleFont;
        private readonly float _yPad;

        static HeaderView ()
        {
            using (var rgb = CGColorSpace.CreateDeviceRGB()){
                nfloat [] colorsBottom = {
                    1, 1, 1, 1f,
                    0.97f, 0.97f, 0.97f, 1f
                };
                Gradient = new CGGradient (rgb, colorsBottom, null);
            }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; SetNeedsDisplay(); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value; SetNeedsDisplay(); }
        }

        public UIImage Image
        {
            get { return _image; }
            set
            {
                if (_image == value)
                    return;
                _image = value; 
                SetNeedsDisplay();
            }
        }

        public string ImageUri
        {
            get { return string.Empty; }
            set 
            {
				_image = string.IsNullOrEmpty(value) ? null : ImageLoader.DefaultRequestImage(new Uri(value), this); 
                SetNeedsDisplay(); 
            }
        }

		public UIColor SeperatorColor { get; set; }
        
		public HeaderView()
			: base(new CGRect(0, 0, 0, 60f))
        {
            BackgroundColor = UIColor.Clear;
			SeperatorColor = UIColor.FromRGB(199, 199, 204);

            _titleFont = TitleFont.WithSize(TitleFont.PointSize * Theme.CurrentTheme.FontSizeRatio);
            if (_titleFont == null)
                _titleFont = TitleFont;

            _subtitleFont = SubtitleFont.WithSize(SubtitleFont.PointSize * Theme.CurrentTheme.FontSizeRatio);
            if (_subtitleFont == null)
                _subtitleFont = SubtitleFont;

            _yPad = YPad;

            if (Theme.CurrentTheme.FontSizeRatio > 1.0f)
            {
                _yPad -= ((Theme.CurrentTheme.FontSizeRatio * 1.2f * YPad) - YPad);
            }

//            Layer.MasksToBounds = false;
//            Layer.ShadowColor = UIColor.Gray.CGColor;
//            Layer.ShadowOpacity = 0.4f;
//            Layer.ShadowOffset = new SizeF(0, 1f);
        }

        public void UpdatedImage(Uri uri)
        {
            Image = ImageLoader.DefaultRequestImage(uri, this);
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            var context = UIGraphics.GetCurrentContext();
            nfloat titleY = string.IsNullOrWhiteSpace(Subtitle) ? rect.Height / 2 - _titleFont.LineHeight / 2 : _yPad;
            nfloat contentWidth = rect.Width - XPad * 2;
            var midx = rect.Width/2;

            UIColor.White.SetColor ();
            context.FillRect (rect);
            context.DrawLinearGradient (Gradient, new CGPoint (midx, 0), new CGPoint (midx, rect.Height), 0);

            if (Image != null)
            {
                var height = Image.Size.Height > 36 ? 36 : Image.Size.Height;
                var width = Image.Size.Width > 36 ? 36 : Image.Size.Width;
                var top = rect.Height / 2 - height / 2;
                var left = rect.Width - XPad - width;
                Image.Draw(new CGRect(left, top, width, height));
                contentWidth -= (width + XPad * 2); 
            }


            if (!string.IsNullOrEmpty(Title))
            {
                Theme.CurrentTheme.MainTitleColor.SetColor();
                new NSString(Title).DrawString(
                        new CGRect(XPad, titleY, contentWidth, _titleFont.LineHeight),
                        _titleFont,
                        UILineBreakMode.TailTruncation
                );
            }

            if (!string.IsNullOrWhiteSpace(Subtitle))
            {
                Theme.CurrentTheme.MainSubtitleColor.SetColor();
                new NSString(Subtitle).DrawString(
                    new CGRect(XPad, _yPad + _titleFont.LineHeight + 2f, contentWidth, _subtitleFont.LineHeight),
                    _subtitleFont,
                    UILineBreakMode.TailTruncation
                );
            }

			context.SetStrokeColor(SeperatorColor.CGColor);
			context.SetLineWidth(1.0f);

			context.BeginPath();
			context.MoveTo(0, 0);
			context.AddLineToPoint(rect.Width, 0);
			context.StrokePath();

			context.BeginPath();
			context.MoveTo(0, rect.Bottom);
			context.AddLineToPoint(rect.Width, rect.Bottom);
			context.StrokePath();
        }
    }
}

