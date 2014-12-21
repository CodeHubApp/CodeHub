using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssignedToView : ReactiveTableViewController<IssueAssignedToViewModel>
    {

        public override void ViewDidLoad()
        {
            //NoItemsText = "No Assignees";

//            base.ViewDidLoad();
//
//            this.BindList(ViewModel.Users, x =>
//			{
//				var el = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
//				el.Tapped += () => {
//					if (ViewModel.SelectedUser != null && string.Equals(ViewModel.SelectedUser.Login, x.Login))
//						ViewModel.SelectedUser = null;
//					else
//						ViewModel.SelectedUser = x;
//				};
//				if (ViewModel.SelectedUser != null && string.Equals(ViewModel.SelectedUser.Login, x.Login, StringComparison.OrdinalIgnoreCase))
//					el.Accessory = UITableViewCellAccessory.Checkmark;
//				else
//					el.Accessory = UITableViewCellAccessory.None;
//				return el;
//			});
//
//			ViewModel.WhenAnyValue(x => x.SelectedUser).Where(x => x != null).Subscribe(x =>
//			{
//				if (Root.Count == 0)
//					return;
//				foreach (var m in Root[0].Cast<UserElement>())
//					m.Accessory = (x != null && string.Equals(ViewModel.SelectedUser.Login, m.Caption, StringComparison.OrdinalIgnoreCase)) ? 
//					          	   UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
//				Root.Reload(Root[0], UITableViewRowAnimation.None);
//			});
        }
    }
}

