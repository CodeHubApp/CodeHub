using System;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;

namespace CodeHub.iOS.DialogElements
{
    public class SplitViewElement : Element, IElementSizing
    {
        public static UIColor DefaulTextColor = UIColor.FromWhiteAlpha(0.1f, 1.0f);
        public static UIFont TextFont = UIFont.SystemFontOfSize(14f);

        public SplitButton Button1 { get; set; }

        public SplitButton Button2 { get; set; }

        public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return 44f;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell("splitelement") as SplitCell ?? new SplitCell();

            var buttons = new List<SplitButton>();
            if (Button1 != null)
            {
                buttons.Add(Button1);
            }
            if (Button2 != null)
            {
                buttons.Add(Button2);
            }

            cell.SetButtons(tv, buttons);

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

            public void SetButtons(UITableView tableView, List<SplitButton> items)
            {
                if (_buttons != null)
                {
                    foreach (var btn in _buttons)
                    {
                        btn.RemoveFromSuperview();
                    }
                }

                _buttons = new UIButton[items.Count];

                for (var i = 0; i < items.Count; i++)
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

                if (items.Count > 0)
                {
                    _seperatorViews = new UIView[items.Count - 1];
                    for (var i = 0; i < _seperatorViews.Length; i++)
                    {
                        _seperatorViews[i] = new UIView { BackgroundColor = tableView.SeparatorColor };
                        ContentView.Add(_seperatorViews[i]);
                    }
                }
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (_buttons != null)
                {
                    var width = this.Bounds.Width;
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

            public UIColor ImageTintColor
            {
                get { return _image.TintColor; }
                set
                {
                    _image.TintColor = value;
                    _image.SetNeedsDisplay();
                }
            }

            public UIColor TextColor
            {
                get { return _text.TextColor; }
                set 
                { 
                    _text.TextColor = value;
                    _text.SetNeedsDisplay();
                }
            }

            public SplitButton(UIImage image, string text, Action touched = null)
                : base(UIButtonType.Custom)
            {
                AutosizesSubviews = true;

                _image = new UIImageView();
                _image.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                _image.TintColor = Theme.PrimaryNavigationBarColor;
                this.Add(_image);

                _text = new UILabel();
                _text.TextColor = DefaulTextColor;
                _text.Font = TextFont;
                _text.Text = text;
                _text.AdjustsFontSizeToFitWidth = true;
                _text.MinimumScaleFactor = 0.7f;
                this.Add(_text);

//                if (touched != null)
//                {
//                    this.TouchDown += (sender, e) => this.BackgroundColor = UIColor.FromWhiteAlpha(0.95f, 1.0f);
//                    this.TouchUpOutside += (sender, e) => this.BackgroundColor = UIColor.White;
//
//                    this.TouchUpInside += (sender, e) => 
//                    {
//                        this.BackgroundColor = UIColor.White;
//                        touched();
//                    };
//                }
            }
            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var height = (this.Bounds.Height - 20f);
                _image.Frame = new CGRect(15, 10, height, height);

                var textHeight = (int)Math.Ceiling(TextFont.LineHeight) + 1;
                var textY = (this.Bounds.Height / 2) - (textHeight / 2);
                _text.Frame = new CGRect(_image.Frame.Right + 5f, textY, (int)Math.Floor(this.Bounds.Width) - (_image.Frame.Right + 5f + _image.Frame.Left), textHeight);
            }
        }
    }
}

