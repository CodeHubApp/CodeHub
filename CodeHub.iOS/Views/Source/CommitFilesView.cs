using System;
using UIKit;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.iOS.Views.Source
{
    public class CommitFilesView : BaseTableViewController<CommitFilesViewModel>
    {
        public CommitFilesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            this.WhenAnyValue(x => x.ViewModel.Files)
                .Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;
        }
    }
}

