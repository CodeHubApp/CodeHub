using System;
using Humanizer;
using CodeHub.Core.Utilities;
using UIKit;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueOptionSelectorView<T> : BaseTableViewController
    {
        private readonly LoadableReactiveList<T> _items;

        public IssueOptionSelectorView(LoadableReactiveList<T> items)
            : base(UITableViewStyle.Plain)
        {
            _items = items;
        }
    }
}

