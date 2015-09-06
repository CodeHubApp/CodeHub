using UIKit;
using CoreGraphics;
using CoreGraphics;
using System;

namespace MonoTouch.Dialog
{
    public class SplitButtonElement : Element, IElementSizing
    {
        public static UIColor CaptionColor = UIColor.Black;
        public static UIFont CaptionFont = UIFont.BoldSystemFontOfSize(16f);
        public static UIColor TextColor = UIColor.LightGray;
        public static UIFont TextFont = UIFont.SystemFontOfSize(12f);

        private readonly Item[] _items;

        public SplitButtonElement(params Item[] items)
            : base("test")
        {
            _items = items;
        }

        public class Item
        {
            public string Caption;
            public string Text;
            public Action Tapped;
        }


        public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return 44f;
        }

		public override UITableViewCell GetCell(UITableView tv)
		{
            var cell = tv.DequeueReusableCell("splitbuttonelement") as SplitButtonCell;
            if (cell == null)
                cell = new SplitButtonCell();
            cell.SetButtons(_items);

			cell.SeparatorInset = UIEdgeInsets.Zero;
			return cell;
		}

        private class SplitButtonCell : UITableViewCell
        {
            private UIButton[] _buttons;

            public SplitButtonCell()
            {
            }

            public void SetButtons(SplitButtonElement.Item[] items)
            {
                if (_buttons != null)
                    foreach (var btn in _buttons)
                    {
                        btn.RemoveFromSuperview();
                        btn.Dispose();
                    }
                _buttons = new UIButton[items.Length];

                for (var i = 0; i < items.Length; i++)
                {
                    _buttons[i] = new UIButton(UIButtonType.Custom);
                    _buttons[i].AutosizesSubviews = true;
                    _buttons[i].AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                    _buttons[i].AddSubview(new ButtonView(items[i].Caption, items[i].Text));
                    this.AddSubview(_buttons[i]);
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
                        _buttons[i].Frame = new CGRect(i * space, 0, space, this.Bounds.Height);
                    }
                }
            }

        }


        private class ButtonView : UIView
        {
            private readonly UILabel _caption;
            private readonly UILabel _text;

            public ButtonView(string caption, string text)
            {
                AutosizesSubviews = true;
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;


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
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                _caption.Frame = new CGRect(10, 10, this.Bounds.Width - 20, 14);
                _text.Frame = new CGRect(10, 30, this.Bounds.Width - 20, 14);
            }
        }

    }

}

