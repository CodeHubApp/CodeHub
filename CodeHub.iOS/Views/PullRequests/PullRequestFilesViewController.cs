using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.ViewControllers;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestFilesViewController : ViewModelCollectionDrivenViewController
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
//                el.Tapped += () => NavigationController.PushViewController(
//                    new RawContentViewController(x.RawUrl, null) { Title = name }, true);
                return el;
            });
        }

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new CustomSource(this);
        }
    
        private class CustomSource : Source
        {
            public CustomSource(PullRequestFilesViewController parent)
                : base(parent)
            {
            }

            public override MonoTouch.UIKit.UIView GetViewForHeader(MonoTouch.UIKit.UITableView tableView, int sectionIdx)
            {
                var view = base.GetViewForHeader(tableView, sectionIdx);
                foreach (var v in view.Subviews)
                {
                    var label = v as UILabel;
                    if (label != null)
                    {
                        label.LineBreakMode = UILineBreakMode.HeadTruncation;
                    }
                }
                return view;
            }
        }
    }
}



