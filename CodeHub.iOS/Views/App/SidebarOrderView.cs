using System;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.App;
using MonoTouch.Dialog;
using System.Linq;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.App
{
    public class SidebarOrderView : ViewModelDialogViewController<SidebarOrderViewModel>
    {
        public SidebarOrderView()
            : base(style: MonoTouch.UIKit.UITableViewStyle.Plain)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Sidebar Order";
            var vm = (SidebarOrderViewModel)ViewModel;

            Root.Add(new Section
            {
                vm.Items.Select(x => new StyledStringElement(x))
            });

            this.Editing = true;
        }

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new EditingSource(this);
        }

        private class EditingSource : Source
        {
            public EditingSource(SidebarOrderView ctrl)
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

