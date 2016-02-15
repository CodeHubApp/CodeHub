using System;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.DialogElements
{
    public class SplitViewElement : Element, IElementSizing
    {
        public static UIColor DefaulTextColor = UIColor.FromWhiteAlpha(0.1f, 1.0f);
        public static UIFont TextFont = UIFont.SystemFontOfSize(14f);

        public SplitButton Button1 { get; }

        public SplitButton Button2 { get; }

        public SplitViewElement(UIImage image1, UIImage image2, string text1 = null, string text2 = null)
        {
            Button1 = new SplitButton(image1, text1);
            Button2 = new SplitButton(image2, text2);
        }

        public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return 44f;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell("splitelement") as SplitCell ?? new SplitCell();
            cell.SetButtons(tv, new [] { Button1, Button2 });
            cell.SeparatorInset = UIEdgeInsets.Zero;
            cell.PreservesSuperviewLayoutMargins = false;
            cell.LayoutMargins = UIEdgeInsets.Zero;
            return cell;
        }

        private class SplitCell : UITableViewCell
        {
            private readonly static float SeperatorWidth = 1.0f;
            private UIButton[] _buttons;
            private UIView[] _seperatorViews;

            static SplitCell()
            {
                if (UIScreen.MainScreen.Scale > 1.0f)
                    SeperatorWidth = 0.5f;
            }

            public SplitCell()
                : base(UITableViewCellStyle.Default, "splitelement")
            {
                SelectionStyle = UITableViewCellSelectionStyle.None;
            }

            public void SetButtons(UITableView tableView, SplitButton[] items)
            {
                if (_buttons != null)
                {
                    foreach (var btn in _buttons)
                    {
                        btn.RemoveFromSuperview();
                    }
                }

                _buttons = new UIButton[items.Length];

                for (var i = 0; i < items.Length; i++)
                {
                    _buttons[i] = items[i];
                    ContentView.Add(_buttons[i]);
                }

                if (_seperatorViews != null)
                {
                    foreach (var v in _seperatorViews)
                    {
                        v.RemoveFromSuperview();
                        v.Dispose();
                    }
                    _seperatorViews = null;
                }

                _seperatorViews = new UIView[Math.Max(items.Length - 1, 0)];
                for (var i = 0; i < _seperatorViews.Length; i++)
                {
                    _seperatorViews[i] = new UIView { BackgroundColor = tableView.SeparatorColor };
                    ContentView.Add(_seperatorViews[i]);
                }
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (_buttons == null)
                    return;

                var width = Bounds.Width;
                var space = width / _buttons.Length;

                for (var i = 0; i < _buttons.Length; i++)
                {
                    _buttons[i].Frame = new CGRect(i * space, 0, space - 1f, Bounds.Height);
                    _buttons[i].LayoutSubviews();

                    if (i != _buttons.Length - 1)
                        _seperatorViews[i].Frame = new CGRect(_buttons[i].Frame.Right, 0, SeperatorWidth, Bounds.Height);
                }
            }
        }


        public class SplitButton : UIButton
        {
            private readonly UIImageView _image;
            private readonly UILabel _text;

            public string Text
            {
                get { return _text.Text; }
                set { _text.Text = value; }
            }

            public UIImage Image
            {
                get { return _image.Image; }
                set { _image.Image = value; }
            }

            public SplitButton(UIImage image, string text = null)
            {
                AutosizesSubviews = true;

                _image = new UIImageView();
                _image.Image = image;
                this.Add(_image);

                _text = new UILabel();
                _text.TextColor = DefaulTextColor;
                _text.Font = TextFont;
                _text.Text = text ?? string.Empty;
                _text.AdjustsFontSizeToFitWidth = true;
                _text.MinimumScaleFactor = 0.7f;
                this.Add(_text);
            }

            private static bool IsPad = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var offset = IsPad ? 24f : 18f;
                var rightOffset = IsPad ? 16f : 14f;

                var height = (this.Bounds.Height - 24f);
                _image.Frame = new CGRect(offset, 12, height, height);

                var textHeight = (int)Math.Ceiling(TextFont.LineHeight) + 1;
                var textY = (this.Bounds.Height / 2) - (textHeight / 2);
                _text.Frame = new CGRect(_image.Frame.Right + rightOffset, textY, (int)Math.Floor(this.Bounds.Width) - (_image.Frame.Right + rightOffset + _image.Frame.Left), textHeight);
            }
        }
    }
}

