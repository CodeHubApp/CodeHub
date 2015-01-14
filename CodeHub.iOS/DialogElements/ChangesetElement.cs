using CodeFramework.iOS.ViewComponents;

namespace CodeHub.iOS.DialogElements
{
    public class ChangesetElement : StyledStringElement
    {
        private readonly int? _added;
        private readonly int? _removed;

        public ChangesetElement(string title, string subtitle, int? added, int? removed)
            : base(title, subtitle, MonoTouch.UIKit.UITableViewCellStyle.Subtitle)
        {
            Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
            LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation;
            Lines = 1;
            _added = added;
            _removed = removed;
        }

        protected override string GetKey(int style)
        {
            return "changeset";
        }

        protected override MonoTouch.UIKit.UITableViewCell CreateTableViewCell(MonoTouch.UIKit.UITableViewCellStyle style, string key)
        {
            return new ChangesetCell(key);
        }

        public override MonoTouch.UIKit.UITableViewCell GetCell(MonoTouch.UIKit.UITableView tv)
        {
            var cell = base.GetCell(tv);
            var addRemove = ((ChangesetCell)cell).AddRemoveView;
            addRemove.Added = _added;
            addRemove.Removed = _removed;
            addRemove.SetNeedsDisplay();
            return cell;
        }

        /// Bastardized version. I'll redo this code later...
        private class ChangesetCell : MonoTouch.UIKit.UITableViewCell
        {
            public AddRemoveView AddRemoveView { get; private set; }

            public ChangesetCell(string key)
                : base(MonoTouch.UIKit.UITableViewCellStyle.Subtitle, key)
            {
                AddRemoveView = new AddRemoveView();
                this.ContentView.AddSubview(AddRemoveView);
                TextLabel.LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                var addRemoveX = ContentView.Frame.Width - 90f;
                AddRemoveView.Frame = new System.Drawing.RectangleF(addRemoveX, 12, 80f, 18f);

                var textFrame = TextLabel.Frame;
                textFrame.Width = addRemoveX - textFrame.X - 5f;
                TextLabel.Frame = textFrame;
            }
        }
    }
}

