using System;
using UIKit;
using CoreGraphics;

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
            : base(new CGRect(0, 0, 320f, 60f))
        {
            _button = new UIButton(UIButtonType.Custom);
            _button.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);
            _button.Layer.ShadowColor = UIColor.Black.CGColor;
            _button.Layer.ShadowOffset = new CGSize(0, 1);
            _button.Layer.ShadowOpacity = 0.1f;
            _button.TouchUpInside += (sender, e) => ButtonTapped();
            _button.SetTitleColor(UIColor.DarkGray, UIControlState.Normal);
            Add(_button);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _button.Frame = new CGRect(15, 8, Bounds.Width - 30f, Bounds.Height - 16f);
        }
    }
}

