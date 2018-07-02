using System;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using CodeHub.iOS.DialogElements;
using System.Linq;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.ViewControllers
{
    public class DialogViewController : TableViewController
    {
        private readonly Lazy<RootElement> _rootElement;
        private UISearchBar _searchBar;

        public RootElement Root => _rootElement.Value;

        public bool EnableSearch { get; set; }

        public string SearchPlaceholder { get; set; }

        public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate (fromInterfaceOrientation);
            ReloadData ();
        }

        Section [] originalSections;
        Element [][] originalElements;

        /// <summary>
        /// Allows caller to programatically activate the search bar and start the search process
        /// </summary>
        public void StartSearch ()
        {
            if (originalSections != null)
                return;

            _searchBar.BecomeFirstResponder ();
            CreateOriginals(Root);
        }

        private void CreateOriginals(RootElement root)
        {
            originalSections = root.Sections.ToArray ();
            originalElements = new Element [originalSections.Length][];
            for (int i = 0; i < originalSections.Length; i++)
                originalElements [i] = originalSections [i].Elements.ToArray ();
        }

        /// <summary>
        /// Allows the caller to programatically stop searching.
        /// </summary>
        public virtual void FinishSearch ()
        {
            if (originalSections == null)
                return;

            _searchBar.Text = "";

            Root.Reset(originalSections);
            originalSections = null;
            originalElements = null;
            _searchBar.ResignFirstResponder ();
            ReloadData ();
        }

        public void PerformFilter (string text)
        {
            if (originalSections == null)
                return;

            var newSections = new List<Section> ();

            for (int sidx = 0; sidx < originalSections.Length; sidx++){
                Section newSection = null;
                var section = originalSections [sidx];
                Element [] elements = originalElements [sidx];

                for (int eidx = 0; eidx < elements.Length; eidx++){
                    if (elements [eidx].Matches (text)){
                        if (newSection == null){
                            newSection = new Section (section.Header, section.Footer){
                                FooterView = section.FooterView,
                                HeaderView = section.HeaderView
                            };
                            newSections.Add (newSection);
                        }
                        newSection.Add (elements [eidx]);
                    }
                }
            }

            Root.Reset(newSections);
            ReloadData ();
        }

        public virtual void SearchButtonClicked (string text)
        {
            _searchBar.ResignFirstResponder();
        }

        protected class SearchDelegate : UISearchBarDelegate {
            readonly WeakReference<DialogViewController> container;

            public SearchDelegate (DialogViewController container)
            {
                this.container = new WeakReference<DialogViewController>(container);
            }

            public override void OnEditingStarted (UISearchBar searchBar)
            {
                searchBar.ShowsCancelButton = true;
                container.Get()?.StartSearch ();
            }

            public override void OnEditingStopped (UISearchBar searchBar)
            {
                searchBar.ShowsCancelButton = false;
                //container.FinishSearch ();
            }

            public override void TextChanged (UISearchBar searchBar, string searchText)
            {
                container.Get()?.PerformFilter (searchText ?? "");
            }

            public override void CancelButtonClicked (UISearchBar searchBar)
            {
                var r = container.Get();
                searchBar.ShowsCancelButton = false;
                if (r != null)
                {
                    r._searchBar.Text = "";
                    r.FinishSearch();
                }
                searchBar.ResignFirstResponder ();
            }

            public override void SearchButtonClicked (UISearchBar searchBar)
            {
                container.Get()?.SearchButtonClicked (searchBar.Text);
            }
        }

        protected virtual void DidScroll(CGPoint p)
        {
        }

        protected virtual IUISearchBarDelegate CreateSearchDelegate()
        {
            return new SearchDelegate(this);
        }

        void SetupSearch ()
        {
            if (EnableSearch){
                _searchBar = new UISearchBar (new CGRect (0, 0, TableView.Bounds.Width, 44)) {
                    Delegate = CreateSearchDelegate()
                };
                if (SearchPlaceholder != null)
                    _searchBar.Placeholder = this.SearchPlaceholder;
                TableView.TableHeaderView = _searchBar;                    
            } else {
                // Does not work with current Monotouch, will work with 3.0
                // tableView.TableHeaderView = null;
            }
        }

        public virtual void Deselected (NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];

            element.Deselected (TableView, indexPath);
        }

        public virtual void Selected (NSIndexPath indexPath)
        {
            var section = Root[indexPath.Section];
            var element = section[indexPath.Row];

            element.Selected (TableView, indexPath);
        }

        public virtual UITableViewSource CreateSizingSource()
        {
            return new DialogTableViewSource(TableView);
        }

        public override void LoadView ()
        {
            base.LoadView();
            SetupSearch ();
            TableView.Source = CreateSizingSource();
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            TableView.ReloadData();
        }

        public void ReloadData ()
        {
            TableView.ReloadData();
        }

        public DialogViewController (UITableViewStyle style = UITableViewStyle.Plain) 
            : base (style)
        {
            _rootElement = new Lazy<RootElement>(() => new RootElement(TableView));

            EdgesForExtendedLayout = UIRectEdge.None;
            SearchPlaceholder = "Search";
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };
        }
    }
}