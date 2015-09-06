using System;
using System.Linq;
using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssignedToView : ViewModelCollectionDrivenDialogViewController
    {

        public override void ViewDidLoad()
        {
            Title = "Assignees".t();
            NoItemsText = "No Assignees".t();

            base.ViewDidLoad();

			var vm = (IssueAssignedToViewModel)ViewModel;
			BindCollection(vm.Users, x =>
			{
				var el = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
				el.Tapped += () => {
					if (vm.SelectedUser != null && string.Equals(vm.SelectedUser.Login, x.Login))
						vm.SelectedUser = null;
					else
						vm.SelectedUser = x;
				};
				if (vm.SelectedUser != null && string.Equals(vm.SelectedUser.Login, x.Login, StringComparison.OrdinalIgnoreCase))
					el.Accessory = UITableViewCellAccessory.Checkmark;
				else
					el.Accessory = UITableViewCellAccessory.None;
				return el;
			});

			vm.Bind(x => x.SelectedUser, x =>
			{
				if (Root.Count == 0)
					return;
				foreach (var m in Root[0].Elements.Cast<UserElement>())
					m.Accessory = (x != null && string.Equals(vm.SelectedUser.Login, m.Caption, StringComparison.OrdinalIgnoreCase)) ? 
					          	   UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				Root.Reload(Root[0], UITableViewRowAnimation.None);
			});

			var _hud = new Hud(View);
			vm.Bind(x => x.IsSaving, x =>
			{
				if (x) _hud.Show("Saving...");
				else _hud.Hide();
			});
        }
    }
}

