using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using UIKit;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : BaseTableViewController<PullRequestFilesViewModel>
    {
        public PullRequestFilesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            TableView.Source = notificationSource;

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel.Files)
                    .Merge(this.WhenAnyObservable(x => x.ViewModel.Files.Changed).Select(_ => ViewModel.Files))
                    .Subscribe(notificationSource.SetData));
            });
        }
    }
}



