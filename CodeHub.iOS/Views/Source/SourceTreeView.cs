using System;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Source
{
    public class SourceTreeView : BaseTableViewController<SourceTreeViewModel>
    {
        public SourceTreeView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileDirectory.ToEmptyListImage(), "This directory is empty."));

            this.WhenAnyValue(x => x.ViewModel.CanAddFile)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x ? ViewModel.GoToAddFileCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new SourceContentTableViewSource(TableView, ViewModel.Content);
        }
    }
}

