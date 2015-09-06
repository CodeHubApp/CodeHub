using CodeFramework.ViewControllers;
using CodeFramework.Core.ViewModels.App;
using MonoTouch.Dialog;
using UIKit;
using System.Linq;

namespace CodeFramework.iOS.Views.App
{
	public class DefaultStartupView : ViewModelCollectionDrivenDialogViewController
    {
		public DefaultStartupView()
        {
			Title = "Default Startup View".t();
			EnableSearch = false;
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var vm = (BaseDefaultStartupViewModel)ViewModel;
			BindCollection(vm.StartupViews, x => {
				var e = new StyledStringElement(x);
				e.Tapped += () => vm.SelectedStartupView = x;
				if (string.Equals(vm.SelectedStartupView, x))
					e.Accessory = UITableViewCellAccessory.Checkmark;
				return e;
			}, true);

			vm.Bind(x => x.SelectedStartupView, x =>
			{
				if (Root.Count == 0)
					return;
				foreach (var m in Root[0].Elements.Cast<StyledStringElement>())
					m.Accessory = (string.Equals(m.Caption, x)) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				Root.Reload(Root[0], UITableViewRowAnimation.None);
			}, true);
		}
    }
}

