using System;
using System.Reactive.Linq;

// Analysis disable once CheckNamespace
using System.Reactive;


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

        public static IObservable<Unit> GetClickedObservable(this UIBarButtonItem @this)
        {
            return Observable.FromEventPattern(t => @this.Clicked += t, t => @this.Clicked -= t).Select(_ => Unit.Default);
        }

        public static IObservable<Unit> GetChangedObservable(this UIRefreshControl @this)
        {
            return Observable.FromEventPattern(t => @this.ValueChanged += t, t => @this.ValueChanged -= t).Select(_ => Unit.Default);
        }


    }
}

