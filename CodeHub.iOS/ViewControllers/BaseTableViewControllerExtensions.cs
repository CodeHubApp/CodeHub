using System;
using UIKit;
using CoreGraphics;
using CodeHub.iOS.Utilities;
using ReactiveUI;
using CodeHub.Core.Services;
using Splat;

namespace CodeHub.iOS.ViewControllers
{
    public static class BaseTableViewControllerExtensions
    {
        public static IAnalyticsService GetAnalytics(this BaseTableViewController viewController)
        {
            return Locator.Current.GetService<IAnalyticsService>();
        }

        public static void TrackScreen(this BaseTableViewController viewController)
        {
            var analytics = viewController.GetAnalytics();
            if (analytics == null)
                return;

            var screenName = viewController.GetType().Name;
            analytics.Screen(screenName);
        }

        public static ObservableSearchDelegate AddSearchBar(this BaseTableViewController @this)
        {
            UISearchBar searchBar;
            return AddSearchBar(@this, out searchBar);
        }

        public static ObservableSearchDelegate AddSearchBar(this BaseTableViewController @this, out UISearchBar searchBar)
        {
            searchBar = new UISearchBar(new CGRect(0f, 0f, 320f, 44f));
            searchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

            var searchDelegate = new ObservableSearchDelegate();
            searchBar.Delegate = searchDelegate;

            @this.TableView.TableHeaderView = searchBar;

            return searchDelegate;
        }

        public static ObservableSearchDelegate AddSearchBar(this BaseTableViewController @this, Action<string> searchAction)
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

