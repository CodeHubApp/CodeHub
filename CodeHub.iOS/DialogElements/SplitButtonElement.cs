using UIKit;
using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace CodeHub.iOS.DialogElements
{
    public class SplitButtonElement : Element
    {
        public static UIColor CaptionColor = UIColor.FromRGB(41, 41, 41);
        public static UIFont CaptionFont = UIFont.SystemFontOfSize(14f);
        public static UIColor TextColor = UIColor.FromRGB(100, 100, 100);
        public static UIFont TextFont = UIFont.BoldSystemFontOfSize(14f);
        private readonly List<Button> _buttons = new List<Button>();

        public class Button
        {
            private readonly SplitButtonElement _element;
            private string _text, _caption;

            public Button(SplitButtonElement element, string text, string caption)
            {
                _element = element;
                _text = text;
                _caption = caption;
                Clicked = new Subject<Unit>();
            }

            public ISubject<Unit> Clicked { get; }

            public string Text
            {
                get { return _text; }
                set
                {
                    _text = value;
                    _element.GetRootElement()?.Reload(_element);
                }
            }
            public string Caption
            {
                get { return _caption; }
                set
                {
                    _caption = value;
                    _element.GetRootElement()?.Reload(_element);
                }
            }
        }

        public Button AddButton(string caption, string text = null)
        {
            var btn = new Button(this, text, caption);
            _buttons.Add(btn);
            return btn;
        }

        public SplitButtonElement()
        {
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell("splitbuttonelement") as SplitButtonCell;
            if (cell == null)
            {
                cell = new SplitButtonCell(_buttons.Count);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.SeparatorInset = UIEdgeInsets.Zero;
                cell.BackgroundColor = tv.SeparatorColor;
            }
            cell.SetButtons(_buttons);
            return cell;
        }

        private class SplitButtonCell : UITableViewCell
        {
            private readonly static float SeperatorWidth = 1.0f;
            private readonly SplitButton[] _buttons;

            static SplitButtonCell()
            {
                if (UIScreen.MainScreen.Scale > 1.0f)
                    SeperatorWidth = 0.5f;
            }


            public SplitButtonCell(int buttons)
                : base(UITableViewCellStyle.Default, "splitbuttonelement")
            {
                _buttons = Enumerable.Range(0, buttons)
                    .Select(x => new SplitButton())
                    .ToArray();

                foreach (var b in _buttons)
                    ContentView.Add(b);
            }

            public void SetButtons(List<Button> items)
            {
                foreach (var b in _buttons.Zip(items, (x, y) => new { Button = x, Data = y }))
                {
                    b.Button.Caption = b.Data.Caption;
                    b.Button.Text = b.Data.Text;
                    b.Button.UserInteractionEnabled = true;
                    var weakRef = new WeakReference<Button>(b.Data);
                    b.Button.Touch = () => weakRef.Get()?.Clicked.OnNext(Unit.Default);
                }
            }


            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var width = this.Bounds.Width;
                var space = width / (float)_buttons.Length;

                for (var i = 0; i < _buttons.Length; i++)
                {
                    _buttons[i].Frame = new CGRect(i * space, 0, space - SeperatorWidth, Bounds.Height);
                    _buttons[i].LayoutSubviews();
                }
            }
        }


        private class SplitButton : UIButton
        {
            private readonly UILabel _caption;
            private readonly UILabel _text;

            public string Text
            {
                get { return _text.Text; }
                set 
                { 
                    if (_text.Text == value)
                        return;
                    _text.Text = value; 
                    SetNeedsDisplay();
                }
            }

            public string Caption
            {
                get { return _caption.Text; }
                set 
                { 
                    if (_caption.Text == value)
                        return;
                    _caption.Text = value; 
                    SetNeedsDisplay();
                }
            }

            public Action Touch;

            public SplitButton()
            {
                AutosizesSubviews = true;

                BackgroundColor = UIColor.White;

                _caption = new UILabel();
                _caption.TextColor = CaptionColor;
                _caption.Font = CaptionFont;
                this.Add(_caption);

                _text = new UILabel();
                _text.TextColor = TextColor;
                _text.Font = TextFont;
                this.Add(_text);

                this.TouchUpInside += (sender, e) => Touch?.Invoke();
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                _text.Frame = new CGRect(12, 3, (int)Math.Floor(this.Bounds.Width) - 24f, (int)Math.Ceiling(TextFont.LineHeight) + 1);
                _caption.Frame = new CGRect(12, _text.Frame.Bottom, (int)Math.Floor(this.Bounds.Width) - 24f, (int)Math.Ceiling(CaptionFont.LineHeight));
            }
        }

    }

}
