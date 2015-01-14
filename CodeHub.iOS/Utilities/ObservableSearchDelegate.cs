using System;
using MonoTouch.UIKit;
using System.Reactive.Subjects;

namespace CodeHub.iOS.Utilities
{
    public class ObservableSearchDelegate : UISearchBarDelegate 
    {
        private readonly Subject<string> _searchTextChanging = new Subject<string>();
        private readonly Subject<string> _searchTextChanged = new Subject<string>();

        /// <summary>
        /// An observable when the search text is changing (incremental)
        /// </summary>
        public IObservable<string> SearchTextChanging { get { return _searchTextChanging; } }

        /// <summary>
        /// An observable when the search text has finally changed (button press)
        /// </summary>
        /// <value>The search text changed.</value>
        public IObservable<string> SearchTextChanged { get { return _searchTextChanged; } }

        public override void OnEditingStarted (UISearchBar searchBar)
        {
            searchBar.ShowsCancelButton = true;
        }

        public override void OnEditingStopped (UISearchBar searchBar)
        {
            searchBar.ShowsCancelButton = false;
        }

        public override void TextChanged (UISearchBar searchBar, string searchText)
        {
            _searchTextChanging.OnNext(searchText);
        }

        public override void CancelButtonClicked (UISearchBar searchBar)
        {
            searchBar.ShowsCancelButton = false;
            searchBar.ResignFirstResponder ();
            searchBar.Text = string.Empty;
            _searchTextChanging.OnNext(searchBar.Text);
        }

        public override void SearchButtonClicked (UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
            _searchTextChanged.OnNext(searchBar.Text);
        }
    }
}

