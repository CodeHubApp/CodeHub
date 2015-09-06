using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MonoTouch.Dialog.Elements
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

        public MultilinedInputElement(string caption) 
            : base(caption)
        {
        }

        protected override NSString CellKey
        {
            get { return new NSString("multilinedInputElement"); }
        }


        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CellKey) as CustomInputCell;
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
                    var newHeight = (int)GetHeight(GetImmediateRootElement().TableView, this.IndexPath);
                    Console.WriteLine("{0} vs {1}", height, newHeight);
                    if (newHeight != height)
                    {
                        GetImmediateRootElement().TableView.BeginUpdates();
                        GetImmediateRootElement().TableView.EndUpdates();
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
            public static UIFont InputFont = UIFont.SystemFontOfSize(14f);
            public readonly UITextView TextView;

            public CustomInputCell()
                : base(new CGRect(0, 0, 320, 100f))
            {
                TextView = new UITextView(new CGRect(0, 0, ContentView.Frame.Width, ContentView.Frame.Height));
                TextView.ScrollEnabled = false;
                TextView.BackgroundColor = UIColor.Clear;
                TextView.Font = InputFont;
                ContentView.Add(TextView);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                SeparatorInset = new UIEdgeInsets(0, Bounds.Width, 0, 0);
                TextView.Frame = new CGRect(0, 0, ContentView.Frame.Width, ContentView.Frame.Height);
                TextView.LayoutIfNeeded();
            }
        }

        public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
        {
            var str = new NSString(Value ?? string.Empty);
            var height = (int)str.StringSize(CustomInputCell.InputFont, new CGSize(tableView.Bounds.Width, 10000)).Height + 40f;
            Console.WriteLine("Calculated height: " + height);
            return height > 200 ? height : 200;
        }
    }
}