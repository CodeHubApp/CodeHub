using System;
using ReactiveUI;

// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UITableViewControllerExtensions
    {
        public static UIView CreateTopBackground(this UITableViewController viewController, UIColor color)
        {
            var frame = viewController.TableView.Bounds;
            frame.Y = -frame.Size.Height;
            var view = new UIView(frame);
            view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            view.BackgroundColor = color;
            view.Layer.ZPosition = -1f;
            viewController.TableView.InsertSubview(view, 0);
            return view;
        }

        public static IDisposable BindTableSource<TViewModel, TSource>(this IObservable<IReadOnlyReactiveList<TViewModel>> @this, UITableView tableView, 
            Func<UITableView, IReadOnlyReactiveList<TViewModel>, TSource> source)
            where TSource : ReactiveUI.ReactiveTableViewSource<TViewModel>
        {
            return @this.Subscribe(x => {
                if (tableView.Source != null)
                    tableView.Source.Dispose();
                tableView.Source = source(tableView, x);
            });
        }
    }
}