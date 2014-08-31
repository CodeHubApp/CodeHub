using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Events;
using System;
using System.Drawing;
using MonoTouch.Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class EventTableViewSource : ReactiveTableViewSource<EventItemViewModel>
    {
        private NewsCellView _usedForHeight;

        public EventTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<EventItemViewModel> collection) 
            : base(tableView, collection,  NewsCellView.Key, 60.0f)
        {
            tableView.SeparatorInset = NewsCellView.EdgeInsets;
            tableView.RegisterNibForCellReuse(NewsCellView.Nib, NewsCellView.Key);
        }

        private static float CharacterHeight 
        {
            get { return "A".MonoStringHeight(NewsCellView.BodyFont, 1000); }
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = (NewsCellView)tableView.DequeueReusableCell(NewsCellView.Key);

            var item = ItemAt(indexPath) as EventItemViewModel;
            if (item != null)
            {
                var s = 6f + NewsCellView.TimeFont.LineHeight + 5f + (NewsCellView.HeaderFont.LineHeight * 2) + 4f + 7f;
                _usedForHeight.ViewModel = item;

                if (_usedForHeight.BodyString.Length == 0)
                    return s;

                var rec = _usedForHeight.BodyString.GetBoundingRect(new SizeF(tableView.Bounds.Width - 56, 10000), NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading, null);
                var height = rec.Height;

                if (item.BodyBlocks.Count == 1 && height > (CharacterHeight * 4))
                    height = CharacterHeight * 4;

                var descCalc = s + height;
                var ret = ((int)Math.Ceiling(descCalc)) + 1f + 8f;
                return ret;
            }

            return base.GetHeightForRow(tableView, indexPath);
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as EventItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

