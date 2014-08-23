using System;
using CodeHub.iOS.Delegates;
using MonoTouch.UIKit;
using System.Drawing;

namespace ReactiveUI
{
    public static class ReactiveTableViewControllerExtensions
    {
        public static ObservableSearchDelegate AddSearchBar<T>(this ReactiveTableViewController<T> @this) where T : class
        {
            var searchBar = new UISearchBar(new RectangleF(0f, 0f, 320f, 44f));
            searchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

            var searchDelegate = new ObservableSearchDelegate();
            searchBar.Delegate = searchDelegate;

            @this.TableView.TableHeaderView = searchBar;

            return searchDelegate;
        }

        public static ObservableSearchDelegate AddSearchBar<T>(this ReactiveTableViewController<T> @this, Action<string> searchAction) where T : class
        {
            var searchDelegate = AddSearchBar<T>(@this);

            @this.WhenActivated(d =>
            {
                d(searchDelegate.SearchTextChanging.Subscribe(searchAction));
            });

            return searchDelegate;
        }
    }
}

