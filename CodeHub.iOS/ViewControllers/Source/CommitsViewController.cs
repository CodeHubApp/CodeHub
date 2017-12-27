using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using CoreGraphics;
using ReactiveUI;
using UIKit;
using Splat;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class CommitsViewController : UIViewController
    {
        private readonly TableListViewController<Octokit.GitHubCommit> _tableViewCtrl;

        public static CommitsViewController CreatePullRequestCommitsViewController(string owner, string name, int id)
            => new CommitsViewController(Octokit.ApiUrls.PullRequestCommits(owner, name, id));

        public static CommitsViewController CreateBranchCommitsViewController(string owner, string name, string sha)
            => new CommitsViewController(Octokit.ApiUrls.RepositoryCommits(owner, name)); //TODO: FIX THIS MISSING SHA

        public CommitsViewController(
            Uri uri,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            Title = "Commits";

            var paginator = new GitHubPaginator<Octokit.GitHubCommit>(applicationService.GitHubClient, uri);
            _tableViewCtrl = new TableListViewController<Octokit.GitHubCommit>(
                paginator,
                x => new CommitElement(x),
                () => Octicon.GitCommit.ToEmptyListImage());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddChildViewController(_tableViewCtrl);
            View.AddSubview(_tableViewCtrl.View);
        }

        public class CommitElement : Element
        {
            private readonly CommitItemViewModel _viewModel;

            public CommitElement(Octokit.GitHubCommit commit)
            {
                _viewModel = new CommitItemViewModel(commit);
            }

            public override UITableViewCell GetCell(UITableView tv)
            {
                var cell = tv.DequeueReusableCell(CommitCellView.Key) as CommitCellView ?? CommitCellView.Create();
                cell.Set(_viewModel);
                return cell;
            }
        }
    }
}
