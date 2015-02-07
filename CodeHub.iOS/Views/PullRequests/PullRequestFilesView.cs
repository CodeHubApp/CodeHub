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
                new EmptyListView(Octicon.FileCode.ToImage(64f), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            this.WhenAnyValue(x => x.ViewModel.Files.Changed)
                .Switch()
                .Select(_ => ViewModel.Files)
                .Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;
        }
    }
}



