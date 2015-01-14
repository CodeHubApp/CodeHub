using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class MultilinedInputElement : Element, IElementSizing
    {
        public string Value
        {
            get { return _val; }
            set
            {
                _val = value;
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }
        private string _val;
        public event EventHandler ValueChanged;

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CustomInputCell.Key) as CustomInputCell;
            if (cell == null)
            {
                cell = new CustomInputCell();
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                cell.TextView.Ended += delegate {
                    Value = cell.TextView.Text;
                };

                cell.TextView.Changed += (s, e) => {
                    Value = cell.TextView.Text;
                    var height = (int)cell.Bounds.Height;
                    var newHeight = (int)GetHeight(GetRootElement().TableView, this.IndexPath);
                    Console.WriteLine("{0} vs {1}", height, newHeight);
                    if (newHeight != height)
                    {
                        GetRootElement().TableView.BeginUpdates();
                        GetRootElement().TableView.EndUpdates();
                    }

                    //cell.TextView.GetCaretRectForPosition(cell.TextView.SelectedTextRange.start);
                    cell.TextView.ScrollRangeToVisible(cell.TextView.SelectedRange);
                };

                cell.TextView.ReturnKeyType = UIReturnKeyType.Done;
            }

            cell.TextView.Text = Value ?? string.Empty;
            return cell;
        }

        private class CustomInputCell : UITableViewCell
        {
            public static NSString Key = new NSString("CustomInputCell");
            public static UIFont InputFont = UIFont.SystemFontOfSize(14f);
            public readonly UITextView TextView;

            public CustomInputCell()
                : base(UITableViewCellStyle.Default, Key)
            {
                TextView = new UITextView(new RectangleF(0, 0, ContentView.Frame.Width, ContentView.Frame.Height));
                TextView.ScrollEnabled = false;
                TextView.BackgroundColor = UIColor.Clear;
                TextView.Font = InputFont;
                ContentView.Add(TextView);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                SeparatorInset = new UIEdgeInsets(0, Bounds.Width, 0, 0);
                TextView.Frame = new RectangleF(0, 0, ContentView.Frame.Width, ContentView.Frame.Height);
                TextView.LayoutIfNeeded();
            }
        }

        public float GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            var str = new NSString(Value ?? string.Empty);
            var height = (int)str.StringSize(CustomInputCell.InputFont, new SizeF(tableView.Bounds.Width, 10000)).Height + 40f;
            Console.WriteLine("Calculated height: " + height);
            return height > 200 ? height : 200;
        }
    }
}