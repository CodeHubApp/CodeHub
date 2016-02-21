using UIKit;
using CodeHub.iOS.TableViewCells;
using System.Reactive.Subjects;
using System;
using System.Reactive.Linq;
using Foundation;

namespace CodeHub.iOS.DialogElements
{
    public class MultilinedElement : Element
    {
        private readonly Subject<object> _tapped = new Subject<object>();

        private string _caption;
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                if (_caption == value)
                    return;
                
                _caption = value;
                var cell = GetActiveCell() as MultilinedCellView;
                if (cell != null)
                    cell.Caption = value;
            }
        }

        private string _details;
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                if (_details == value)
                    return;
                _details = value;
                var cell = GetActiveCell() as MultilinedCellView;
                if (cell != null)
                    cell.Details = value;
            }
        }

        public IObservable<object> Clicked
        {
            get { return _tapped.AsObservable(); }
        }

        public MultilinedElement(string caption = null, string details = null)
        {
            Caption = caption;
            Details = details;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = tv.DequeueReusableCell(MultilinedCellView.Key) as MultilinedCellView ?? MultilinedCellView.Create();
            cell.Caption = Caption;
            cell.Details = Details;
            return cell;
        }

        public override void Selected (UITableView tableView, NSIndexPath indexPath)
        {
            base.Selected(tableView, indexPath);
            _tapped.OnNext(this);
        }
    }
}

