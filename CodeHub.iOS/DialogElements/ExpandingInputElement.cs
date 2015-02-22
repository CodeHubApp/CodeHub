using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class ExpandingInputElement : Element, IElementSizing
    {
        private string _val;
        public event EventHandler ValueChanged;
        private readonly string _description;
        private IDisposable _textEditEnded;
        private IDisposable _textEditChanged;

        public string Value
        {
            get { return _val; }
            set
            {
                if (string.Equals(_val, value))
                    return;

                _val = value;
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }

        public UIFont Font { get; set; }

        public bool SpellChecking { get; set; }

        public Func<UITextView, UIView> AccessoryView { get; set; }

        public bool HiddenSeperator { get; set; }

        public ExpandingInputElement(string description)
        {
            SpellChecking = true;
            _description = description;
            Font = UIFont.PreferredBody;
            HiddenSeperator = true;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CustomInputCell.Key) as CustomInputCell;
            if (cell == null)
            {
                cell = new CustomInputCell(_description);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.TextView.Font = Font;
                cell.TextView.InputAccessoryView = AccessoryView != null ? AccessoryView(cell.TextView) : new UIView();
                cell.TextView.AutocorrectionType = SpellChecking ? UITextAutocorrectionType.Default : UITextAutocorrectionType.No;
                cell.TextView.SpellCheckingType = SpellChecking ? UITextSpellCheckingType.Default : UITextSpellCheckingType.No;
                cell.TextView.AutocapitalizationType = SpellChecking ? UITextAutocapitalizationType.Sentences : UITextAutocapitalizationType.None;
            }

            cell.HiddenSeperator = HiddenSeperator;

            if (_textEditEnded != null)
                _textEditEnded.Dispose();

            _textEditEnded = Observable.FromEventPattern(x => cell.TextView.Ended += x, x => cell.TextView.Ended -= x)
                .Subscribe(x => Value = cell.TextView.Text);

            if (_textEditChanged != null)
                _textEditChanged.Dispose();

            _textEditChanged = Observable.FromEventPattern(x => cell.TextView.Changed += x, x => cell.TextView.Changed -= x)
                .Subscribe(x => 
                {
                    Value = cell.TextView.Text;

                    tv.BeginUpdates();
                    tv.EndUpdates();

                    var caret = cell.TextView.GetCaretRectForPosition(cell.TextView.SelectedTextRange.Start);
                    var cursorRect = tv.ConvertRectFromView(caret, cell.TextView);
                    var kk = cursorRect.Size;
                    kk.Height += 8.0f;
                    cursorRect.Size = kk;
                    tv.ScrollRectToVisible(cursorRect, false);
                });

            cell.TextView.Text = Value ?? string.Empty;
            return cell;
        }

        private class CustomInputCell : UITableViewCell
        {
            public static NSString Key = new NSString("CustomInputCell");
            public static UIFont InputFont = UIFont.PreferredBody;
            public readonly UITextView TextView;

            public bool HiddenSeperator { get; set; }

            public CustomInputCell(string placeholder)
                : base(UITableViewCellStyle.Default, Key)
            {
                HiddenSeperator = true;
                TextView = new CustomTextView(placeholder)
                { 
                    Frame = new CGRect(12, 0, ContentView.Frame.Width - 24f, ContentView.Frame.Height),
                    ScrollEnabled = false,
                };
                TextView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                TextView.BackgroundColor = UIColor.Clear;
                TextView.Font = InputFont;
                ContentView.Add(TextView);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (HiddenSeperator)
                    SeparatorInset = new UIEdgeInsets(0, Bounds.Width, 0, 0);
                else
                    SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
            }

            private class CustomTextView : UITextView
            {
                private readonly UILabel _placeholderView = new UILabel();
                public string Placeholder { get; set; }

                public override UIFont Font
                {
                    get
                    {
                        return base.Font;
                    }
                    set
                    {
                        base.Font = value;
                        _placeholderView.Font = value;
                    }
                }

                public CustomTextView(string placeholder)
                {
                    _placeholderView.Text = placeholder;
                    _placeholderView.TextColor = UIColor.FromWhiteAlpha(0.702f, 1.0f);
                    _placeholderView.Frame = new CGRect(5, 8, 200f, 16f);
                    _placeholderView.UserInteractionEnabled = false;
                    _placeholderView.Font = UIFont.PreferredBody;
                    _placeholderView.Hidden = Text.Length > 0;
                    this.Add(_placeholderView);

                    this.Changed += (sender, e) =>
                        _placeholderView.Hidden = !string.IsNullOrEmpty(Text);
                }
            }
        }

        public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            var str = new NSString(Value ?? string.Empty);
            var height = (int)str.StringSize(CustomInputCell.InputFont, new CGSize(tableView.Bounds.Width - 24f, 10000), UILineBreakMode.WordWrap).Height + 60f;
            return height > 60 ? height : 60;
        }
    }
}

