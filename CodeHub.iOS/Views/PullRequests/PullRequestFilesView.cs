using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : BaseTableViewController<PullRequestFilesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var notificationSource = new CommitedFilesTableViewSource(TableView);
            ViewModel.WhenAnyValue(x => x.Files).Where(x => x != null).Subscribe(notificationSource.SetData);
            TableView.Source = notificationSource;
        }
    }
}



