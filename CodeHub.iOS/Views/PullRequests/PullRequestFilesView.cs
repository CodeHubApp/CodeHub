using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.ViewControllers;
using MonoTouch.Dialog;
using UIKit;
using System;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Files";
            NoItemsText = "No Files".t();

            base.ViewDidLoad();

            var vm = (PullRequestFilesViewModel) ViewModel;
            BindCollection(vm.Files, x =>
            {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                var el = new StyledStringElement(name, x.Status, UITableViewCellStyle.Subtitle);
                el.Image = Images.File;
                el.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				el.Tapped += () =>  vm.GoToSourceCommand.Execute(x);
                return el;
            });
        }

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
        {
            return new CustomSource(this);
        }
    
		private class CustomSource : BaseDialogViewController.Source
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



