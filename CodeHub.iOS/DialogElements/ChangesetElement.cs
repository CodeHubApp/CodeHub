using CodeHub.iOS.Views;

namespace CodeHub.iOS.DialogElements
{
    public class ChangesetElement : StringElement
    {
        private readonly int? _added;
        private readonly int? _removed;

        public ChangesetElement(string title, string subtitle, int? added, int? removed)
            : base(title, subtitle, UIKit.UITableViewCellStyle.Subtitle)
        {
            Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator;
            LineBreakMode = UIKit.UILineBreakMode.TailTruncation;
            Lines = 1;
            _added = added;
            _removed = removed;
        }

        protected override string GetKey(int style)
        {
            return "changeset";
        }

        protected override UIKit.UITableViewCell CreateTableViewCell(UIKit.UITableViewCellStyle style, string key)
        {
            return new ChangesetCell(key);
        }

        public override UIKit.UITableViewCell GetCell(UIKit.UITableView tv)
        {
            var cell = base.GetCell(tv);
            var addRemove = ((ChangesetCell)cell).AddRemoveView;
            addRemove.Added = _added;
            addRemove.Removed = _removed;
            addRemove.SetNeedsDisplay();
            return cell;
        }

        /// Bastardized version. I'll redo this code later...
        private sealed class ChangesetCell : UIKit.UITableViewCell
        {
            public AddRemoveView AddRemoveView { get; private set; }

            public ChangesetCell(string key)
                : base(UIKit.UITableViewCellStyle.Subtitle, key)
            {
                AddRemoveView = new AddRemoveView();
                ContentView.AddSubview(AddRemoveView);
                TextLabel.LineBreakMode = UIKit.UILineBreakMode.TailTruncation;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                var addRemoveX = ContentView.Frame.Width - 90f;
                var addRemoveY = (ContentView.Frame.Height / 2) - 9f;
                AddRemoveView.Frame = new CoreGraphics.CGRect(addRemoveX, addRemoveY, 80f, 18f);

                var textFrame = TextLabel.Frame;
                textFrame.Width = addRemoveX - textFrame.X - 5f;
                TextLabel.Frame = textFrame;
            }
        }
    }
}

