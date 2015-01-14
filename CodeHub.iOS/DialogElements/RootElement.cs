using System;
using MonoTouch.Foundation;
using System.Collections.Generic;
using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class RootElement : IEnumerable<Section> 
    {
        private readonly List<Section> _sections = new List<Section>();
        private readonly IDictionary<string, object> _offscreenCells = new Dictionary<string, object>();
        private readonly UITableView _tableView;

        public UITableView TableView
        {
            get { return _tableView; }
        }

        public IDictionary<string, object> OffscreenCells
        {
            get { return _offscreenCells; }
        }

        public TCell GetOffscreenCell<TCell>(string key, Func<TCell> create) where TCell : class
        {
            TCell cell = default(TCell);
            if (_offscreenCells.ContainsKey(key))
                cell = _offscreenCells[key] as TCell;

            if (cell == default(TCell))
            {
                cell = create();
                _offscreenCells[key] = cell;
            }

            return cell;
        }

        public RootElement(UITableView tableView)
        {
            _tableView = tableView;
        }

        public int Count 
        { 
            get { return _sections.Count; }
        }

        public Section this [int idx] 
        {
            get { return _sections[idx]; }
        }

        internal int IndexOf (Section target)
        {
            int idx = 0;
            foreach (Section s in _sections){
                if (s == target)
                    return idx;
                idx++;
            }
            return -1;
        }

        /// <summary>
        /// Adds a new section to this RootElement
        /// </summary>
        /// <param name="section">
        /// The section to add, if the root is visible, the section is inserted with no animation
        /// </param>
        public void Add (Section section)
        {
            if (section == null)
                return;

            _sections.Add (section);
            section.Root = this;
            _tableView.InsertSections (MakeIndexSet (_sections.Count-1, 1), UITableViewRowAnimation.None);
        }
 
        public void Add (IEnumerable<Section> sections)
        {
            foreach (var s in sections)
                Add (s);
        }

        public void Add(params Section[] sections)
        {
            Add((IEnumerable<Section>)sections);
        }

        NSIndexSet MakeIndexSet (int start, int count)
        {
            NSRange range;
            range.Location = start;
            range.Length = count;
            return NSIndexSet.FromNSRange (range);
        }
   
        public void Insert (int idx, UITableViewRowAnimation anim, params Section [] newSections)
        {
            if (idx < 0 || idx > _sections.Count)
                return;
            if (newSections == null)
                return;

            if (_tableView != null)
                _tableView.BeginUpdates ();

            int pos = idx;
            foreach (var s in newSections){
                s.Root = this;
                _sections.Insert (pos++, s);
            }

            if (_tableView == null)
                return;

            _tableView.InsertSections (MakeIndexSet (idx, newSections.Length), anim);
            _tableView.EndUpdates ();
        }

        public void Insert (int idx, Section section)
        {
            Insert (idx, UITableViewRowAnimation.None, section);
        }

        public void RemoveAt (int idx)
        {
            RemoveAt (idx, UITableViewRowAnimation.Fade);
        }

        public void RemoveAt (int idx, UITableViewRowAnimation anim)
        {
            if (idx < 0 || idx >= _sections.Count)
                return;

            _sections.RemoveAt (idx);

            if (_tableView == null)
                return;

            _tableView.DeleteSections (NSIndexSet.FromIndex (idx), anim);
        }

        public void Remove (Section s)
        {
            if (s == null)
                return;
            int idx = _sections.IndexOf (s);
            if (idx == -1)
                return;
            RemoveAt (idx, UITableViewRowAnimation.Fade);
        }

        public void Remove (Section s, UITableViewRowAnimation anim)
        {
            if (s == null)
                return;
            int idx = _sections.IndexOf (s);
            if (idx == -1)
                return;
            RemoveAt (idx, anim);
        }

        public void Clear ()
        {
            foreach (var s in _sections)
                s.Root = null;
            _sections.Clear();
            if (_tableView != null)
                _tableView.ReloadData ();
        }

        public void Reset(IEnumerable<Section> sections)
        {
            foreach (var s in _sections)
                s.Root = null;
            _sections.Clear();

            foreach (var s in sections)
            {
                s.Root = this;
                _sections.Add(s);
            }

            if (_tableView != null)
                _tableView.ReloadData ();
        }

        public void Reset(params Section[] sections)
        {
            Reset((IEnumerable<Section>)sections);
        }

        public void Reload (Section section, UITableViewRowAnimation animation = UITableViewRowAnimation.None)
        {
            if (section == null)
                throw new ArgumentNullException ("section");
            if (section.Root == null || section.Root != this)
                throw new ArgumentException ("Section is not attached to this root");

            int idx = 0;
            foreach (var sect in _sections)
            {
                if (sect == section)
                {
                    try
                    {
                        _tableView.BeginUpdates();
                        _tableView.ReloadSections (new NSIndexSet ((uint) idx), animation);
                    }
                    finally
                    {
                        _tableView.EndUpdates();
                    }
                    return;
                }
                idx++;
            }
        }

        public void Reload (Element element, UITableViewRowAnimation animation = UITableViewRowAnimation.None)
        {
            if (element == null)
                throw new ArgumentNullException ("element");
            if (element.Section == null || element.Section.Root == null)
                return;
            if (element.Section.Root != this)
                throw new ArgumentException ("Element is not attached to this root");
            var path = element.IndexPath;
            if (path == null)
                return;

            try
            {
                _tableView.BeginUpdates();
                _tableView.ReloadRows (new [] { path }, animation);
            }
            finally
            {
                _tableView.EndUpdates();
            }
        }

        public IEnumerator<Section> GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

