using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : ReactiveTableViewController<PullRequestFilesViewModel>
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



