using System;
using UIKit;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class CommitFilesViewController : BaseTableViewController<CommitFilesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            this.WhenAnyValue(x => x.ViewModel.Files)
                .Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;
        }
    }
}

