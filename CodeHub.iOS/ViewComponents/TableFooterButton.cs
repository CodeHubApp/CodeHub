using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace CodeHub.iOS.ViewComponents
{
    public class TableFooterButton : UIView
    {
        private readonly UIButton _button;

        public event Action ButtonTapped;

        public string Title
        {
            get { return _button.Title(UIControlState.Normal); }
            set { _button.SetTitle(value, UIControlState.Normal); }
        }

        public TableFooterButton(string title, Action tapped)
            : this()
        {
            Title = title;
            ButtonTapped += tapped;
        }

        public TableFooterButton()
            : base(new RectangleF(0, 0, 320f, 64f))
        {
            _button = new UIButton(UIButtonType.Custom);
            _button.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            _button.Layer.ShadowColor = UIColor.Black.CGColor;
            _button.Layer.ShadowOffset = new SizeF(0, 1);
            _button.Layer.ShadowOpacity = 0.1f;
            _button.TouchUpInside += (sender, e) => ButtonTapped();
            _button.Frame = new RectangleF(15, 12, this.Frame.Width - 30f, this.Frame.Height - 24f);
            _button.SetTitleColor(UIColor.DarkGray, UIControlState.Normal);
            _button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            Add(_button);
        }
    }
}

