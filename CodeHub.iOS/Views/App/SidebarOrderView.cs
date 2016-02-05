using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using System.Linq;

namespace CodeHub.iOS.Views.App
{
    public class SidebarOrderView : ViewModelDrivenDialogViewController
    {
        public SidebarOrderView()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Sidebar Order";
            var vm = (SidebarOrderViewModel)ViewModel;

            var root = new RootElement(Title);
            root.Add(new Section
            {
                vm.Items.Select(x => new StyledStringElement(x))
            });

            Root = root;

            Style = UIKit.UITableViewStyle.Plain;
            this.Editing = true;
        }

        public override DialogViewController.Source CreateSizingSource(bool unevenRows)
        {
            return new EditingSource(this);
        }

        private class EditingSource : DialogViewController.Source
        {
            public EditingSource(DialogViewController ctrl)
                : base(ctrl)
            {
            }

            public override bool CanEditRow(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return true;
            }

            public override void MoveRow(UIKit.UITableView tableView, Foundation.NSIndexPath sourceIndexPath, Foundation.NSIndexPath destinationIndexPath)
            {

            }
        }
    }
}

