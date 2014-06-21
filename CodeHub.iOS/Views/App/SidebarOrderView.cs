using System;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using System.Linq;

namespace CodeHub.iOS.Views.App
{
    public class SidebarOrderView : ViewModelDialogView<SidebarOrderViewModel>
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

            Style = MonoTouch.UIKit.UITableViewStyle.Plain;
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

            public override bool CanEditRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                return true;
            }

            public override void MoveRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath sourceIndexPath, MonoTouch.Foundation.NSIndexPath destinationIndexPath)
            {

            }
        }
    }
}

