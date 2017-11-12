using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Source;
using CodeHub.iOS.ViewControllers.PullRequests;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : ViewModelCollectionDrivenDialogViewController
    {
        public new PullRequestFilesViewModel ViewModel
        {
            get { return (PullRequestFilesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public PullRequestFilesView()
        {
            Title = "Files";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var weakVm = new WeakReference<PullRequestFilesViewModel>(ViewModel);
            var weakVc = new WeakReference<PullRequestFilesView>(this);
            BindCollection(ViewModel.Files, x =>
            {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                var el = new StringElement(name, x.Status, UITableViewCellStyle.Subtitle);
                el.Image = Octicon.FileCode.ToImage();
                el.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                el.Clicked.Subscribe(_ => weakVc.Get()?.GoToFile(x));
                return el;
            });
        }

        private void GoToFile(GitHubSharp.Models.CommitModel.CommitFileModel file)
        {
            if (file.Patch == null)
            {
                var viewController = new FileSourceViewController(
                    ViewModel.Username, ViewModel.Repository, file.Filename, ViewModel.Sha, Utilities.ShaType.Hash);
                this.PushViewController(viewController);
            }
            else
            {
                var viewController = new PullRequestDiffViewController(
                    ViewModel.Username, ViewModel.Repository, (int)ViewModel.PullRequestId, file.Filename,
                    file.Patch, ViewModel.Sha);
                this.PushViewController(viewController);
            }
        }

        public override DialogViewController.Source CreateSizingSource()
        {
            return new CustomSource(this);
        }
    
        private class CustomSource : DialogViewController.Source
        {
            public CustomSource(PullRequestFilesView parent)
                : base(parent)
            {
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var x = headerView as UITableViewHeaderFooterView;
                if (x != null)
                {
                    x.TextLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
                }
            }
        }
    }
}



