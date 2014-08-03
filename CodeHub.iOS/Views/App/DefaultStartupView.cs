using System;
using System.Linq;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.iOS.Views.App
{
    public class DefaultStartupView : ViewModelCollectionViewController<DefaultStartupViewModel>
	{
        public DefaultStartupView()
            : base(searchbarEnabled: false)
		{
            Title = "Default Startup View";
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.BindList(ViewModel.StartupViews, x =>
            {
                var e = new StyledStringElement(x);
                e.Tapped += () => ViewModel.SelectedStartupView = x;
                if (string.Equals(ViewModel.SelectedStartupView, x))
                    e.Accessory = UITableViewCellAccessory.Checkmark;
                return e;
            });

		    ViewModel.WhenAnyValue(x => x.SelectedStartupView).Subscribe(x =>
		    {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Cast<StyledStringElement>())
                    m.Accessory = (string.Equals(m.Caption, x, StringComparison.OrdinalIgnoreCase)) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                Root.Reload(Root[0]);
		    });
		}
    }
}

