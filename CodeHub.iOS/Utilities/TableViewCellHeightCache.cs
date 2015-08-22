using System;
using Foundation;
using System.Collections.Generic;
using UIKit;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.Utilities
{
    public class TableViewCellHeightCache<TCell, TViewModel> 
        where TCell : ReactiveTableViewCell<TViewModel> 
        where TViewModel : class
    {
        private readonly Lazy<TCell> _cachedCell;
        private readonly IDictionary<Tuple<int, int>, nfloat> _heightCache;
        private readonly nfloat _defaultHeight;

        public nfloat this[NSIndexPath path]
        {
            get
            {
                var key = Tuple.Create(path.Section, path.Row);
                if (_heightCache.ContainsKey(key))
                    return _heightCache[key];
                return _defaultHeight;
            }
        }


        public TableViewCellHeightCache(nfloat defaultHeight, Func<TCell> cachedCell)
        {
            _cachedCell = new Lazy<TCell>(cachedCell);
            _heightCache = new Dictionary<Tuple<int, int>, nfloat>(100);
            _defaultHeight = defaultHeight;
        }

        public nfloat Add(NSIndexPath path, nfloat height)
        {
            var key = Tuple.Create(path.Section, path.Row);
            _heightCache[key] = height;
            return height;
        }

        public void Clear()
        {
            _heightCache.Clear();
        }

        public nfloat GenerateHeight(UITableView tableView, TViewModel viewModel, NSIndexPath path)
        {
            var key = Tuple.Create(path.Section, path.Row);
            if (_heightCache.ContainsKey(key))
                return _heightCache[key];

            var cachedCell = _cachedCell.Value;
            cachedCell.ViewModel = viewModel;
            cachedCell.SetNeedsUpdateConstraints();
            cachedCell.UpdateConstraintsIfNeeded();
            cachedCell.Bounds = new CoreGraphics.CGRect(0, 0, tableView.Bounds.Width, tableView.Bounds.Height);
            cachedCell.SetNeedsLayout();
            cachedCell.LayoutIfNeeded();
            var height = cachedCell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height + 1;

            _heightCache[key] = height;
            return height;
        }
    }
}

