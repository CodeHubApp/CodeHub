using System;
using MonoTouch.UIKit;
using System.Drawing;
using CodeHub.iOS.Utilities;

// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public static class ReactiveTableViewControllerExtensions
    {
        public static ObservableSearchDelegate AddSearchBar(this ReactiveTableViewController @this)
        {
            var searchBar = new UISearchBar(new RectangleF(0f, 0f, 320f, 44f));
            searchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

            var searchDelegate = new ObservableSearchDelegate();
            searchBar.Delegate = searchDelegate;

            @this.TableView.TableHeaderView = searchBar;

            return searchDelegate;
        }

        public static ObservableSearchDelegate AddSearchBar(this ReactiveTableViewController @this, Action<string> searchAction)
        {
            var searchDelegate = AddSearchBar(@this);

            var supportsActivation = @this as IActivatable;
            if (supportsActivation != null)
            {
                supportsActivation.WhenActivated(d =>
                {
                    d(searchDelegate.SearchTextChanging.Subscribe(searchAction));
                });
            }
            else
            {
                searchDelegate.SearchTextChanging.Subscribe(searchAction);
            }

            return searchDelegate;
        }
    }
}

