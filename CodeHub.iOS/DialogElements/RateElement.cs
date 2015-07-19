using System;
using UIKit;
using CoreGraphics;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.DialogElements
{
    public class RateElement : Element, IElementSizing
    {
        public RateElement()
        {
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            return tv.DequeueReusableCell(RateTableViewCell.Key) as RateTableViewCell ?? new RateTableViewCell();
        }

        public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return 80f;
        }

        private class RateTableViewCell : UITableViewCell
        {
            public readonly static string Key = "ratecell";
            private readonly AskView _firstAskView;
            private readonly AskView _secondAskView;
            private readonly AskView _reviewAskView;

            public RateTableViewCell() 
                : base(UITableViewCellStyle.Default, Key)
            {
                BackgroundColor = UIColor.Clear;
                SelectionStyle = UITableViewCellSelectionStyle.None;

                _reviewAskView = new AskView();
                _reviewAskView.Label.Text = "Would you mind leaving a review?";
                _reviewAskView.PositiveButton.Label.Text = "Sure!";
                _reviewAskView.NegativeButton.Label.Text = "No thanks!";

                _firstAskView = new AskView();
                _firstAskView.Label.Text = "Are you enjoying CodeHub?";
                _firstAskView.PositiveButton.Label.Text = "Yes!";
                _firstAskView.NegativeButton.Label.Text = "No!";
                _firstAskView.PositiveButton.TouchUpInside += (sender, e) => FadeToView(_firstAskView, _reviewAskView);
                _firstAskView.NegativeButton.TouchUpInside += (sender, e) => FadeToView(_firstAskView, _secondAskView);

                ContentView.Add(_firstAskView);

                _secondAskView = new AskView();
                _secondAskView.Label.Text = "Want to help make it better?";
                _secondAskView.PositiveButton.Label.Text = "Yes please!";
                _secondAskView.NegativeButton.Label.Text = "No thanks";

                AutosizesSubviews = true;
                ContentView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                ContentView.AutosizesSubviews = true;
            }

            private void FadeToView(UIView fromView, UIView toView)
            {
                UIView.Animate(0.15, 0, UIViewAnimationOptions.CurveEaseIn, () => fromView.Alpha = 0, () => {
                    fromView.RemoveFromSuperview();
                    toView.Alpha = 0;
                    ContentView.Add(toView);
                    UIView.Animate(0.15, 0, UIViewAnimationOptions.CurveEaseOut, () => toView.Alpha = 1, null);
                });
            }
        }


        private class AskView : UIView
        {
            public readonly PixelButton PositiveButton;
            public readonly PixelButton NegativeButton;
            public readonly UILabel Label;

            public AskView()
                : base(new CGRect(0, 0, 320, 60))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

                Label = new UILabel();
                Label.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
                Label.TextAlignment = UITextAlignment.Center;
                Label.Frame = new CGRect(10, 10, Frame.Width - 20f, 20f); 
                Label.TextColor = UIColor.White;
                Add(Label);

                PositiveButton = new PixelButton(UIColor.White);
                PositiveButton.Frame = new CGRect(10, Label.Frame.Bottom + 10f, 140f, 30f);
                PositiveButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
                Add(PositiveButton);

                NegativeButton = new PixelButton(UIColor.White);
                NegativeButton.Layer.MasksToBounds = true;
                NegativeButton.Layer.CornerRadius = 6f;
                NegativeButton.Layer.BorderColor = UIColor.White.CGColor;
                NegativeButton.Layer.BorderWidth = 1f;
                NegativeButton.Frame = new CGRect(170, Label.Frame.Bottom + 10f, 140f, 30f);
                NegativeButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin;
                Add(NegativeButton);
            }
        }
    }
}

