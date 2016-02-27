using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : ViewModelCollectionDrivenDialogViewController
    {
        public PullRequestFilesView()
        {
            Title = "Files";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (PullRequestFilesViewModel) ViewModel;
            var weakVm = new WeakReference<PullRequestFilesViewModel>(vm);
            BindCollection(vm.Files, x =>
            {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                var el = new StringElement(name, x.Status, UITableViewCellStyle.Subtitle);
                el.Image = Octicon.FileCode.ToImage();
                el.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                el.Clicked.Subscribe(_ => weakVm.Get()?.GoToSourceCommand.Execute(x));
                return el;
            });
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



