using System.Collections.Generic;
using UIKit;
using CoreGraphics;
using Foundation;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class Section : IEnumerable<Element> {
        object header, footer;
        private readonly List<Element> _elements = new List<Element> ();

        public RootElement Root
        {
            get;
            internal set;
        }

        public IReadOnlyList<Element> Elements
        {
            get { return new ReadOnlyCollection<Element>(_elements); }
        }

        // X corresponds to the alignment, Y to the height of the password
        public CGSize EntryAlignment;

        public Section()
        {
        }

        public Section(UIView header, UIView footer = null)
        {
            HeaderView = header;
            FooterView = footer;
        }

        public Section(string header, string footer = null)
        {
            Header = header;
            Footer = footer;
        }

        /// <summary>
        ///    The section header, as a string
        /// </summary>
        public string Header {
            get {
                return header as string;
            }
            set {
                header = value;
            }
        }

        /// <summary>
        /// The section footer, as a string.
        /// </summary>
        public string Footer {
            get {
                return footer as string;
            }

            set {
                footer = value;
            }
        }

        /// <summary>
        /// The section's header view.  
        /// </summary>
        public UIView HeaderView {
            get {
                return header as UIView;
            }
            set {
                header = value;
            }
        }

        /// <summary>
        /// The section's footer view.
        /// </summary>
        public UIView FooterView {
            get {
                return footer as UIView;
            }
            set {
                footer = value;
            }
        }

        public void Add (Element element)
        {
            if (element == null)
                return;

            Root?.TableView?.BeginUpdates();

            _elements.Add(element);
            element.SetSection(this);

            InsertVisual (_elements.Count - 1, UITableViewRowAnimation.Automatic, 1);
            Root?.TableView?.EndUpdates();
        }

        public void Add(IEnumerable<Element> elements)
        {
            Root?.TableView?.BeginUpdates();

            var count = 0;
            foreach (var e in elements.Where(x => x != null))
            {
                _elements.Add(e);
                e.SetSection(this);
                count++;
            }

            InsertVisual (_elements.Count - count, UITableViewRowAnimation.Automatic, count);
            Root?.TableView?.EndUpdates();
        }

        public void Insert (int idx, UITableViewRowAnimation anim, params Element [] newElements)
        {
            if (newElements == null)
                return;

            int pos = idx;
            foreach (var e in newElements)
            {
                _elements.Insert (pos++, e);
                e.SetSection(this);
            }

            if (Root != null && Root.TableView != null)
            {
                if (anim == UITableViewRowAnimation.None)
                    Root.TableView.ReloadData();
                else
                    InsertVisual (idx, anim, newElements.Length);
            }
        }

        public int Insert (int idx, UITableViewRowAnimation anim, IEnumerable<Element> newElements)
        {
            if (newElements == null)
                return 0;

            int pos = idx;
            int count = 0;
            foreach (var e in newElements)
            {
                _elements.Insert (pos++, e);
                e.SetSection(this);
                count++;
            }

            if (Root != null && Root.TableView != null)
            {                
                if (anim == UITableViewRowAnimation.None)
                    Root.TableView.ReloadData ();
                else
                    InsertVisual (idx, anim, pos-idx);
            }
            return count;
        }

        void InsertVisual (int idx, UITableViewRowAnimation anim, int count)
        {
            if (Root == null || Root.TableView == null)
                return;

            int sidx = Root.IndexOf (this);
            var paths = new NSIndexPath [count];
            for (int i = 0; i < count; i++)
                paths [i] = NSIndexPath.FromRowSection (idx+i, sidx);
            Root.TableView.InsertRows (paths, anim);
        }

        public void Insert (int index, params Element [] newElements)
        {
            Insert (index, UITableViewRowAnimation.None, newElements);
        }

        public void Remove (Element e, UITableViewRowAnimation animation = UITableViewRowAnimation.Automatic)
        {
            if (e == null)
                return;

            for (int i = _elements.Count; i > 0;)
            {
                i--;
                if (_elements [i] == e)
                {
                    RemoveRange (i, 1, animation);
                    e.SetSection(null);
                    return;
                }
            }
        }

        public void Remove (int idx)
        {
            RemoveRange (idx, 1);
        }

        /// <summary>
        /// Remove a range of elements from the section with the given animation
        /// </summary>
        /// <param name="start">
        /// Starting position
        /// </param>
        /// <param name="count">
        /// Number of elements to remove form the section
        /// </param>
        /// <param name="anim">
        /// The animation to use while removing the elements
        /// </param>
        public void RemoveRange (int start, int count, UITableViewRowAnimation anim = UITableViewRowAnimation.Fade)
        {
            if (start < 0 || start >= _elements.Count)
                return;
            if (count == 0)
                return;

            if (start+count > _elements.Count)
                count = _elements.Count-start;

            _elements.RemoveRange (start, count);

            if (Root != null && Root.TableView != null)
            {
                int sidx = Root.IndexOf(this);
                var paths = new NSIndexPath [count];
                for (int i = 0; i < count; i++)
                    paths[i] = NSIndexPath.FromRowSection(start + i, sidx);
                Root.TableView.DeleteRows(paths, anim);

                //Root.TableView.ReloadData();
            }
        }

        public int Count 
        {
            get { return _elements.Count; }
        }

        public IEnumerator<Element> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Element this [int idx] 
        {
            get { return _elements[idx]; }
        }

        public void Clear ()
        {
            foreach (var e in _elements)
                e.SetSection(null);
            _elements.Clear();
            if (Root != null && Root.TableView != null)
                Root.TableView.ReloadData ();
        }

        public void Reset(IEnumerable<Element> elements, UITableViewRowAnimation animation = UITableViewRowAnimation.Fade)
        {
            _elements.Clear();
            _elements.AddRange(elements);
            foreach (var e in _elements)
                e.SetSection(this);
            if (Root != null && Root.TableView != null)
                Root.TableView.ReloadData();
        }
    }
}

