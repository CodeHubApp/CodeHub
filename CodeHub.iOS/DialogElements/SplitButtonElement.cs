using MonoTouch.UIKit;
using System.Drawing;
using System;
using System.Collections.Generic;
using ReactiveUI;

namespace CodeHub.iOS.DialogElements
{
    public class SplitButtonElement : Element
    {
        public static UIColor CaptionColor = UIColor.FromRGB(41, 41, 41);
        public static UIFont CaptionFont = UIFont.SystemFontOfSize(14f);
        public static UIColor TextColor = UIColor.FromRGB(100, 100, 100);
        public static UIFont TextFont = UIFont.BoldSystemFontOfSize(14f);
        private readonly List<SplitButton> _buttons = new List<SplitButton>();

        public SplitButton AddButton(string caption, string text, IReactiveCommand tapped)
        {
            return AddButton(caption, text, () => tapped.ExecuteIfCan());
        }

        public SplitButton AddButton(string caption, string text, Action tapped = null)
        {
            var btn = new SplitButton(caption, text);
            if (tapped != null)
                btn.TouchUpInside += (sender, e) => tapped();
            else
                btn.UserInteractionEnabled = false;

            _buttons.Add(btn);
            return btn;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell("splitbuttonelement") as SplitButtonCell;
            if (cell == null)
            {
                cell = new SplitButtonCell();
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.SeparatorInset = UIEdgeInsets.Zero;
                cell.BackgroundColor = tv.SeparatorColor;
            }
            cell.SetButtons(tv, _buttons);
            return cell;
        }

        private class SplitButtonCell : UITableViewCell
        {
            private readonly static float SeperatorWidth = 1.0f;
            private UIButton[] _buttons;

            static SplitButtonCell()
            {
                if (UIScreen.MainScreen.Scale > 1.0f)
                    SeperatorWidth = 0.5f;
            }


            public SplitButtonCell()
                : base(UITableViewCellStyle.Default, "splitbuttonelement")
            {
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
            }


            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (_buttons != null)
                {
                    var width = this.Bounds.Width;
                    var space = width / (float)_buttons.Length;

                    for (var i = 0; i < _buttons.Length; i++)
                    {
                        _buttons[i].Frame = new RectangleF(i * space, 0, space - SeperatorWidth, Bounds.Height);
                        _buttons[i].LayoutSubviews();
                    }
                }
            }

        }


        public class SplitButton : UIButton
        {
            private readonly UILabel _caption;
            private readonly UILabel _text;

            public string Text
            {
                get { return _text.Text; }
                set 
                { 
                    _text.Text = value; 
                    this.SetNeedsDisplay();
                }
            }

            public SplitButton(string caption, string text)
                : base(UIButtonType.Custom)
            {
                AutosizesSubviews = true;

                BackgroundColor = UIColor.White;

                _caption = new UILabel();
                _caption.TextColor = CaptionColor;
                _caption.Font = CaptionFont;
                _caption.Text = caption;
                this.Add(_caption);

                _text = new UILabel();
                _text.TextColor = TextColor;
                _text.Font = TextFont;
                _text.Text = text;
                this.Add(_text);

                this.TouchDown += (sender, e) => this.BackgroundColor = UIColor.FromWhiteAlpha(0.95f, 1.0f);
                this.TouchUpInside += (sender, e) => this.BackgroundColor = UIColor.White;
                this.TouchUpOutside += (sender, e) => this.BackgroundColor = UIColor.White;
            }
            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                _text.Frame = new RectangleF(12, 3, (int)Math.Floor(this.Bounds.Width) - 24f, (int)Math.Ceiling(TextFont.LineHeight) + 1);
                _caption.Frame = new RectangleF(12, _text.Frame.Bottom, (int)Math.Floor(this.Bounds.Width) - 24f, (int)Math.Ceiling(CaptionFont.LineHeight));
            }
        }

    }

}

