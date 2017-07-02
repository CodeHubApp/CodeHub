using UIKit;
using Foundation;
using System;
using System.Reactive.Subjects;
using CoreGraphics;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;

namespace CodeHub.iOS.TableViewSources
{
    public class DialogTableViewSource : UITableViewSource
    {
        private readonly RootElement _root;
        private readonly Subject<CGPoint> _scrolledSubject = new Subject<CGPoint>();
        private readonly Subject<Element> _selectedSubject = new Subject<Element>();

        public IObservable<CGPoint> ScrolledObservable { get { return _scrolledSubject.AsObservable(); } }

        public IObservable<Element> SelectedObservable { get { return _selectedSubject.AsObservable(); } }

        public RootElement Root
        {
            get { return _root; }
        }

        #if DEBUG
        ~DialogTableViewSource()
        {
            Console.WriteLine("Goodbye DialogTableViewSource");
        }
        #endif

        public DialogTableViewSource(UITableView container)
        {
            container.RowHeight = UITableView.AutomaticDimension;
            _root = new RootElement(container);
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Root[(int)section].Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return Root.Count;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return Root[(int)section].Header;
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return Root[(int)section].Footer;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];
            return element.GetCell(tableView);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];
            element.Deselected(tableView, indexPath);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];
            element.Selected(tableView, indexPath);
            _selectedSubject.OnNext(element);
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var sectionElement = Root[(int)section];
            return sectionElement.HeaderView;
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            var sectionElement = Root[(int)section];
            return sectionElement.HeaderView == null ? -1 : sectionElement.HeaderView.Frame.Height;
        }

        public override UIView GetViewForFooter(UITableView tableView, nint section)
        {
            var sectionElement = Root[(int)section];
            return sectionElement.FooterView;
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var sectionElement = Root[(int)section];
            return sectionElement.FooterView == null ? -1 : sectionElement.FooterView.Frame.Height;
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _scrolledSubject.OnNext(Root.TableView.ContentOffset);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];
            var sizable = element as IElementSizing;
            return sizable == null ? tableView.RowHeight : sizable.GetHeight(tableView, indexPath);
        }
    }
}
