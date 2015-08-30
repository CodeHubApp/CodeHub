using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestFilesViewController : BaseTableViewController<PullRequestFilesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            TableView.Source = notificationSource;

            this.WhenAnyValue(x => x.ViewModel.Files)
                .Merge(this.WhenAnyObservable(x => x.ViewModel.Files.Changed).Select(_ => ViewModel.Files))
                .Subscribe(notificationSource.SetData);
        }
    }
}



