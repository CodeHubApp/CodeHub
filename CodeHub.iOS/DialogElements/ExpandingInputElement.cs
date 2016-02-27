using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using System.Reactive.Subjects;

namespace CodeHub.iOS.DialogElements
{
    public class ExpandingInputElement : Element, IElementSizing
    {
        private string _val;
        private readonly string _description;
        private IDisposable _textEditEnded;
        private IDisposable _textEditChanged;
        private readonly Subject<string> _changedSubject = new Subject<string>();

        public string Value
        {
            get { return _val; }
            set
            {
                if (_val == value)
                    return;

                _val = value;
                _changedSubject.OnNext(value);

                var cell = GetActiveCell() as CustomInputCell;
                if (cell != null)
                    cell.TextView.Text = value;
            }
        }

        public IObservable<string> Changed
        {
            get { return _changedSubject.AsObservable(); }
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
                TextView = new ExtendedUITextView()
                { 
                    Frame = new CGRect(12, 0, ContentView.Frame.Width - 24f, ContentView.Frame.Height),
                    ScrollEnabled = false,
                    Placeholder = placeholder
                };

                TextView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                TextView.BackgroundColor = UIColor.Clear;
                TextView.Font = InputFont;
                TextView.TextColor = UIColor.Black;
                ContentView.Add(TextView);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                SeparatorInset = new UIEdgeInsets(0, HiddenSeperator ? Bounds.Width : 0, 0, 0);
            }
        }

        public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            var str = new NSString(Value ?? string.Empty);
            var height = (int)str.StringSize(CustomInputCell.InputFont, new CGSize(tableView.Bounds.Width - 24f, 10000), UILineBreakMode.WordWrap).Height + 60f;
            return height > 120 ? height : 120;
        }
    }
}

