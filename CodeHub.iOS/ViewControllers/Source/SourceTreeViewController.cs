using System;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class SourceTreeViewController : BaseTableViewController<SourceTreeViewModel>
    {
        public SourceTreeViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileDirectory.ToEmptyListImage(), "This directory is empty."));

            this.WhenAnyValue(x => x.ViewModel.CanAddFile)
                .Select(x => x ? ViewModel.GoToAddFileCommand.ToBarButtonItem(UIBarButtonSystemItem.Add) : null)
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new SourceContentTableViewSource(TableView, ViewModel.Content);
        }
    }
}

