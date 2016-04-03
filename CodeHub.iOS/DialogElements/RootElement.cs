using System;
using Foundation;
using System.Collections.Generic;
using UIKit;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class RootElement : IEnumerable<Section> 
    {
        private readonly List<Section> _sections = new List<Section>();
        private readonly WeakReference<UITableView> _tableView;

        public UITableView TableView
        {
            get { return _tableView.Get(); }
        }

        public IReadOnlyList<Section> Sections
        {
            get { return new ReadOnlyCollection<Section>(_sections); }
        }

        public RootElement(UITableView tableView)
        {
            _tableView = new WeakReference<UITableView>(tableView);
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
            _tableView.Get()?.InsertSections (MakeIndexSet (_sections.Count-1, 1), UITableViewRowAnimation.None);
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

            _tableView.Get()?.BeginUpdates ();

            int pos = idx;
            foreach (var s in newSections){
                s.Root = this;
                _sections.Insert (pos++, s);
            }

            _tableView.Get()?.InsertSections (MakeIndexSet (idx, newSections.Length), anim);
            _tableView.Get()?.EndUpdates ();
        }

        public void Insert (int idx, Section section)
        {
            Insert (idx, UITableViewRowAnimation.None, section);
        }

        public void RemoveAt (int idx, UITableViewRowAnimation anim)
        {
            if (idx < 0 || idx >= _sections.Count)
                return;

            _sections.RemoveAt (idx);
            _tableView.Get()?.DeleteSections (NSIndexSet.FromIndex (idx), anim);
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
            _tableView.Get()?.ReloadData ();
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

            _tableView.Get()?.ReloadData();
        }

        public void Reset(params Section[] sections)
        {
            Reset((IEnumerable<Section>)sections);
        }

//        public void Reload (Section section, UITableViewRowAnimation animation = UITableViewRowAnimation.Automatic)
//        {
//            if (section == null)
//                throw new ArgumentNullException ("section");
//            if (section.Root == null || section.Root != this)
//                throw new ArgumentException ("Section is not attached to this root");
//
//            int idx = 0;
//            foreach (var sect in _sections)
//            {
//                if (sect == section)
//                {
//                    try
//                    {
//                        _tableView.Get()?.BeginUpdates();
//                        _tableView.Get()?.ReloadSections (new NSIndexSet ((uint) idx), animation);
//                    }
//                    finally
//                    {
//                        _tableView.Get()?.EndUpdates();
//                    }
//                    return;
//                }
//                idx++;
//            }
//        }

        public void Reload (Element element)
        {
            Reload(new [] { element });
        }

        public void Reload (Element[] elements)
        {
            var paths = (elements ?? Enumerable.Empty<Element>())
                .Where(x => x.Section != null && x.Section.Root != null)
                .Select(x => x.IndexPath);

            try
            {
                _tableView.Get()?.BeginUpdates();
                _tableView.Get()?.ReloadRows(paths.ToArray(), UITableViewRowAnimation.None);
            }
            finally
            {
                _tableView.Get()?.EndUpdates();
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

