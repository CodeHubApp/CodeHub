using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesView : ViewModelCollectionView<PullRequestFilesViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Files";
            NoItemsText = "No Files";

            base.ViewDidLoad();

            Bind(ViewModel.WhenAnyValue(x => x.Files), x =>
            {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                var el = new StyledStringElement(name, x.Status, UITableViewCellStyle.Subtitle)
                {
                    Image = Images.File,
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                };
                el.Tapped += () =>  ViewModel.GoToSourceCommand.Execute(x);
                return el;
            });
        }

		public override Source CreateSizingSource(bool unevenRows)
        {
            return new CustomSource(this);
        }
    
		private class CustomSource : Source
        {
            public CustomSource(DialogViewController parent)
                : base(parent)
            {
            }

			public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, int section)
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



