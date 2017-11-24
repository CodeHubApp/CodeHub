using System;
using System.Reactive.Linq;
using System.Reactive;
using Foundation;
using System.Linq;
using System.Reactive.Disposables;

// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIKitExtensions
    {
        public static IObservable<int> GetChangedObservable(this UISegmentedControl @this)
        {
            return Observable.FromEventPattern(t => @this.ValueChanged += t, t => @this.ValueChanged -= t).Select(_ => (int)@this.SelectedSegment);
        }

        public static IObservable<string> GetChangedObservable(this UITextField @this)
        {
            return Observable.FromEventPattern(t => @this.EditingChanged += t, t => @this.EditingChanged -= t).Select(_ => @this.Text);
        }

        public static IObservable<string> GetChangedObservable(this UITextView @this)
        {
            return Observable.FromEventPattern(t => @this.Changed += t, t => @this.Changed -= t).Select(_ => @this.Text);
        }

        public static IObservable<Unit> GetClickedObservable(this UIButton @this)
        {
            return Observable.FromEventPattern(t => @this.TouchUpInside += t, t => @this.TouchUpInside -= t).Select(_ => Unit.Default);
        }

        public static IObservable<UIBarButtonItem> GetClickedObservable(this UIBarButtonItem @this)
        {
            return Observable.FromEventPattern(t => @this.Clicked += t, t => @this.Clicked -= t).Select(_ => @this);
        }

        public static IObservable<Unit> GetChangedObservable(this UIRefreshControl @this)
        {
            return Observable.FromEventPattern(t => @this.ValueChanged += t, t => @this.ValueChanged -= t).Select(_ => Unit.Default);
        }

        public static IObservable<string> GetChangedObservable(this UISearchBar @this)
        {
            return Observable.FromEventPattern<UISearchBarTextChangedEventArgs>(t => @this.TextChanged += t, t => @this.TextChanged -= t).Select(_ => @this.Text);
        }

        public static IObservable<Unit> GetSearchObservable(this UISearchBar @this)
        {
            return Observable.FromEventPattern(t => @this.SearchButtonClicked += t, t => @this.SearchButtonClicked -= t).Select(_ => Unit.Default);
        }

        public static void SetLoading(this UIActivityIndicatorView @this, bool loading)
        {
            if (loading)
                @this.StartAnimating();
            else
                @this.StopAnimating();
        }

        public static IDisposable DisableInteraction(this UIApplication application)
        {
            application.BeginIgnoringInteractionEvents();
            return Disposable.Create(application.EndIgnoringInteractionEvents);
        }

        public static string GetVersion(this UIApplication _) 
        {
            string shortVersion = string.Empty;
            string bundleVersion = string.Empty;

            try
            {
                shortVersion = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            }
            catch { }

            try
            {
                bundleVersion = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            }
            catch { }

            if (string.Equals(shortVersion, bundleVersion))
                return shortVersion;

            return string.IsNullOrEmpty(bundleVersion) ? shortVersion : string.Format("{0} ({1})", shortVersion, bundleVersion);
        }

        public static bool LastItemVisible(this UITableView tableView)
        {
            var paths = tableView.IndexPathsForVisibleRows;
            var sections = tableView.NumberOfSections();
            if (sections == 0)
                return true;

            var rows = tableView.NumberOfRowsInSection(sections - 1);
            if (rows == 0)
                return true;

            var indexPath = NSIndexPath.FromItemSection(rows - 1, sections - 1);
            return paths.Contains(indexPath);
        }

        public static void PushViewController(this UIViewController This, UIViewController view)
            => This.NavigationController?.PushViewController(view, true);

        public static void PresentModalViewController(this UIViewController This, UIViewController view)
        {
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            view.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(
                _ => This.DismissViewController(true, null));
                
            var navController = new UINavigationController(view);
            This.PresentViewController(navController, true, null);
        }
    }

    public static class UIFontExtensions
    {
        public static UIFont MakeBold(this UIFont font)
        {
            var descriptor = font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold);
            if (descriptor == null)
                return font;

            return UIFont.FromDescriptor(descriptor, font.PointSize);
        }

        public static UIFont MakeItalic(this UIFont font)
        {
            var descriptor = font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Italic);
            if (descriptor == null)
                return font;
            
            return UIFont.FromDescriptor(descriptor, font.PointSize);
        }
    }

}

