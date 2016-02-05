using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using System;

namespace CodeHub.iOS.Views
{
    public class ScrollingToolbarView : UIView
    {
        private readonly static float XPADDING;
        private readonly static float XOFFSET;
        private readonly UIScrollView _scrollView;
        private readonly IEnumerable<UIButton> _buttons;

        static ScrollingToolbarView()
        {
            XPADDING = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 10 : 15f;
            XOFFSET = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 5 : 10;
        }

        public ScrollingToolbarView(CGRect rect, IEnumerable<UIButton> buttons)
            : base(rect)
        {
            _buttons = buttons;
            this.AutosizesSubviews = true;
            _scrollView = new UIScrollView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
            _scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _scrollView.UserInteractionEnabled = true;
            _scrollView.ExclusiveTouch = true;
            _scrollView.CanCancelContentTouches = true;
            _scrollView.DelaysContentTouches = true;
            _scrollView.ShowsHorizontalScrollIndicator = false;
            _scrollView.ShowsVerticalScrollIndicator = false;

            var line = new UIView(new CGRect(0, 0, rect.Width, 0.5f));
            line.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
            line.BackgroundColor = UIColor.DarkGray;
            Add(line);

            foreach (var button in buttons)
                _scrollView.Add(button);

            Add(_scrollView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            foreach (var button in _buttons)
                button.SizeToFit();

            nfloat left = -6f;
            foreach (var button in _buttons)
            {
                var frame = button.Frame;
                frame.X = XPADDING + left;
                frame.Y = 5f;
                frame.Height = Bounds.Height - 10f;

                if (frame.Width < frame.Height)
                {
                    if (frame.Height - frame.Width < XOFFSET)
                        frame.Width = frame.Height + XPADDING;
                    else
                        frame.Width = frame.Height;
                }
                else
                {
                    frame.Width = frame.Width + XPADDING;
                }

                button.Frame = frame;
                left = button.Frame.Right;
            }

            _scrollView.ContentSize = new CGSize(left + XPADDING, this.Frame.Height);
        }
    }
}

