using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public abstract class Element 
    {
        internal Section Section;

        public string Caption { get; set; }

        protected Element()
        {
        }

        public abstract UITableViewCell GetCell(UITableView tv);

        public virtual void Deselected (UITableView tableView, NSIndexPath path)
        {
        }

        public virtual void Selected (UITableView tableView, NSIndexPath path)
        {
            tableView.DeselectRow (path, true);
        }

        public RootElement GetRootElement()
        {
            var section = Section;
            return section == null ? null : section.Root;
        }

        public UITableView GetContainerTableView()
        {
            var root = GetRootElement ();
            return root == null ? null : root.TableView;
        }

        public UITableViewCell GetActiveCell()
        {
            var tv = GetContainerTableView();
            if (tv == null)
                return null;
            var path = IndexPath;
            return path == null ? null : tv.CellAt(path);
        }

        public NSIndexPath IndexPath { 
            get 
            {
                if (Section == null || Section.Root == null)
                    return null;

                int row = 0;
                foreach (var element in Section)
                {
                    if (element == this)
                    {
                        int nsect = 0;
                        foreach (var sect in Section.Root)
                        {
                            if (Section == sect){
                                return NSIndexPath.FromRowSection (row, nsect);
                            }
                            nsect++;
                        }
                    }
                    row++;
                }
                return null;
            }
        }

        public virtual bool Matches (string text)
        {
            if (Caption == null)
                return false;
            return Caption.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1;
        }
    }
}

